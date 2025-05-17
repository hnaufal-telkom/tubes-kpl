using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Service
{
    public class LeaveService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRepository;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public LeaveService(ILeaveRequestRepository leaveRepository, IUserService userService, ILogger logger)
        {
            _leaveRepository = leaveRepository;
            _userService = userService;
            _logger = logger;
        }

        public LeaveRequest SubmitRequest(int userId, DateTime startDate, DateTime endDate, string description)
        {
            try
            {
                _logger.Information("Submitting leave request for user {UserId}", userId);
                if (startDate < DateTime.Now)
                {
                    _logger.Error("Start date cannot be in the past");
                    throw new ArgumentException("Start date cannot be in the past");
                }
                if (startDate > endDate)
                {
                    _logger.Error("Start date cannot be after end date");
                    throw new ArgumentException("Start date cannot be after end date");
                }

                var user = _userService.GetUserById(userId);
                var duration = (endDate - startDate).Days + 1;

                if (user.RemainingLeaveDays < duration)
                {
                    _logger.Error("User does not have enough leave days");
                    throw new ArgumentException("User does not have enough leave days P");
                }

                var allUsers = _leaveRepository.GetAll().ToList();
                int newId = 0;
                while (allUsers.Any(u => u.Id == newId))
                {
                    newId++;
                }

                var request = new LeaveRequest
                {
                    Id = newId,
                    UserId = userId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Description = description,
                    Status = RequestStatus.Pending,
                    RequestDate = DateTime.Now,
                };

                _leaveRepository.Add(request);
                _userService.UpdateUser(user);
                _logger.Information("Leave request submitted successfully for user {UserId}", userId);
                return request;
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("User not found");
                throw new KeyNotFoundException("User not found");
            }
            catch (ArgumentException)
            {
                _logger.Error("Invalid Request");
                throw new ArgumentException("Invalid Request");
            }
        }

        public void ApproveRequest(int userId, int approverId)
        {
            try
            {
                _logger.Information("Approving leave request {RequestId} by approver {ApproverId}", userId, approverId);
                var approver = _userService.GetUserById(approverId);
                if (RoleExtensions.CanApproveLeave(approver.Role) || RoleExtensions.CanManageSystem(approver.Role))
                {
                    _logger.Error("User is not an approver");
                    throw new InvalidOperationException("User is not an approver");
                }

                var request = _leaveRepository.GetById(userId);
                if (request.Status != RequestStatus.Pending)
                {
                    _logger.Error("Leave request is not pending");
                    throw new InvalidOperationException("Leave request is not pending");
                }

                var user = _userService.GetUserById(userId);
                var duration = request.Duration;
                user.RemainingLeaveDays -= duration;

                request.Status = RequestStatus.Approved;
                request.ApproverId = approverId;
                request.ApprovalDate = DateTime.Now;

                _userService.UpdateUser(user);
                _leaveRepository.Update(request);
                _logger.Information("Leave request approved successfully by approver {ApproverId}", approverId);
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("Leave request not found");
                throw new KeyNotFoundException("Leave request not found");
            }
        }

        public void RejectRequest(int requestId, int approverId, string reason)
        {
            try
            {
                _logger.Information("Rejecting leave request {RequestId} by approver {ApproverId}", requestId, approverId);
                var approver = _userService.GetUserById(approverId);
                if (RoleExtensions.CanApproveLeave(approver.Role) || RoleExtensions.CanManageSystem(approver.Role))
                {
                    _logger.Error("User is not an approver");
                    throw new InvalidOperationException("User is not an approver");
                }
                var request = _leaveRepository.GetById(requestId);
                if (request.Status != RequestStatus.Pending)
                {
                    _logger.Error("Leave request is not pending");
                    throw new InvalidOperationException("Leave request is not pending");
                }
                request.Status = RequestStatus.Rejected;
                request.RejectionReason = reason;
                request.ApprovalDate = DateTime.Now;
                _leaveRepository.Update(request);
                _logger.Information("Leave request rejected successfully by approver {ApproverId}", approverId);
            }
            catch (KeyNotFoundException)
            {
                _logger.Error("Leave request not found");
                throw new KeyNotFoundException("Leave request not found");
            }
        }

        public IEnumerable<LeaveRequest> GetAllRequests()
        {
            _logger.Information("Getting all leave requests");
            return _leaveRepository.GetAll();
        }

        public IEnumerable<LeaveRequest> GetPendingRequest()
        {
            _logger.Information("Getting all pending leave requests");
            return _leaveRepository.GetPendingRequests();
        }

        public LeaveRequest GetById(int requestId)
        {
            _logger.Information("Getting leave request by ID {RequestId}", requestId);
            return _leaveRepository.GetById(requestId);
        }

        public IEnumerable<LeaveRequest> GetByUserId(int userId)
        {
            _logger.Information("Getting leave requests for user {UserId}", userId);
            return _leaveRepository.GetByUserId(userId);
        }
    }
}
