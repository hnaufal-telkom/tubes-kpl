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
            _logger = logger.ForContext<LeaveService>();
        }

        public LeaveRequest SubmitRequest(int userId, DateTime startDate, DateTime endDate, string description)
        {
            _logger.Information("Submitting leave request for user {UserId}", userId);

            ValidateLeaveDates(startDate, endDate);

            var user = _userService.GetUserById(userId);
            var duration = CalculateDuration(startDate, endDate);

            ValidateAvailableLeaveDays(user, duration);

            var request = CreateLeaveRequest(userId, startDate, endDate, description);

            _leaveRepository.Add(request);
            _logger.Information("Leave request submitted successfully for user {UserId}", userId);

            return request;
        }

        public void ApproveRequest(int requestId, int approverId)
        {
            _logger.Information("Approving leave request {RequestId} by approver {ApproverId}", requestId, approverId);

            var approver = ValidateApprover(approverId);
            var request = GetAndValidatePendingRequest(requestId);
            var user = _userService.GetUserById(request.UserId);

            UpdateLeaveRequestStatus(request, RequestStatus.Approved, approverId);
            DeductLeaveDays(user, request.Duration);

            _logger.Information("Leave request approved successfully by approver {ApproverId}", approverId);
        }

        public void RejectRequest(int requestId, int approverId, string reason)
        {
            _logger.Information("Rejecting leave request {RequestId} by approver {ApproverId}", requestId, approverId);

            ValidateApprover(approverId);
            var request = GetAndValidatePendingRequest(requestId);

            UpdateLeaveRequestStatus(request, RequestStatus.Rejected, approverId, reason);

            _logger.Information("Leave request rejected successfully by approver {ApproverId}", approverId);
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

        public LeaveRequest GetById(int requestId) => _leaveRepository.GetById(requestId);

        public IEnumerable<LeaveRequest> GetByUserId(int userId)
        {
            _logger.Information("Getting leave requests for user {UserId}", userId);
            return _leaveRepository.GetByUserId(userId);
        }

        #region Private Helper Methods

        private void ValidateLeaveDates(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.Today)
            {
                _logger.Error("Start date cannot be in the past");
                throw new ArgumentException("Start date cannot be in the past");
            }

            if (startDate > endDate)
            {
                _logger.Error("Start date cannot be after end date");
                throw new ArgumentException("Start date cannot be after end date");
            }
        }

        private int CalculateDuration(DateTime startDate, DateTime endDate) =>
            (endDate - startDate).Days + 1;

        private void ValidateAvailableLeaveDays(User user, int duration)
        {
            if (user.RemainingLeaveDays < duration)
            {
                _logger.Error("User does not have enough leave days");
                throw new ArgumentException("User does not have enough leave days");
            }
        }

        private LeaveRequest CreateLeaveRequest(int userId, DateTime startDate, DateTime endDate, string description) =>
            new LeaveRequest
            {
                Id = _leaveRepository.GenerateId(),
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                Status = RequestStatus.Pending,
                RequestDate = DateTime.Now
            };

        private User ValidateApprover(int approverId)
        {
            var approver = _userService.GetUserById(approverId);

            if (!RoleExtensions.CanApproveLeave(approver.Role))
            {
                _logger.Error("User {ApproverId} is not an approver", approverId);
                throw new InvalidOperationException("User is not an approver");
            }

            return approver;
        }

        private LeaveRequest GetAndValidatePendingRequest(int requestId)
        {
            var request = _leaveRepository.GetById(requestId);

            if (request.Status != RequestStatus.Pending)
            {
                _logger.Error("Leave request {RequestId} is not pending", requestId);
                throw new InvalidOperationException("Leave request is not pending");
            }

            return request;
        }

        private void UpdateLeaveRequestStatus(LeaveRequest request, RequestStatus status, int approverId, string rejectionReason = null)
        {
            request.Status = status;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;

            if (status == RequestStatus.Rejected)
            {
                request.RejectionReason = rejectionReason;
            }

            _leaveRepository.Update(request);
        }

        private void DeductLeaveDays(User user, int duration)
        {
            user.RemainingLeaveDays -= duration;
            _userService.UpdateUser(user);
        }

        #endregion
    }
}