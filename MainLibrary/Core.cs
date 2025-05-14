using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace MainLibrary
{
    public enum RequestStatus { Pending, Approved, Rejected, Cancelled }
    public enum Role { Employee, HRD, Supervisor, Finance, SysAdmin }


    public class User
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public Role Role { get; set; }
        public DateTime JoinDate { get; set; }
        public int RemainingLeaveDays { get; set; } = 12;
        public bool IsActive { get; set; } = true;
    }

    public class LeaveRequest
    {
        public string? Id { get; set; }
        public required string UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Description { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string? ApproverId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? RejectionReason { get; set; }
        public int Duration => (EndDate - StartDate).Days + 1;
    }

    public class BusinessTrip
    {
        public string? Id { get; set; }
        public required string UserId { get; set; }
        public required string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Purpose { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; } = 0;
        public string? ApproverId { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
    }

    public class Payroll
    {
        public string? Id { get; set; }
        public required string UserId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal MealAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TotalSalary => BasicSalary + TravelAllowance + MealAllowance + OtherAllowances;
        public DateTime PaymentDate { get; set; }
        public bool IsPaid { get; set; } = false;
    }

    public interface IUserRepository
    {
        User GetById(string id);
        IEnumerable<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(string id);
        bool Exists(string email);
        User GetByEmail(string email);
        string GenerateId();
    }

    public interface IUserService
    {
        User Register(string name, string email, string password, Role role);
        User? Authenticate(string email, string password);
        User? GetAllUser();
        User GetUserById(string userId);
        void UpdateUserDetails(string userId, string name, string email);
        void ChangePassword(string userId, string currentPassword, string newPassword);
        void DeactivateUser(string userId);
        void PromoteUser(string userId, Role newRole);
        void ResetPassword(string userId, string newPassword);
        void UpdateUser(User user);
    }

    public interface ILeaveRequestRepository
    {
        LeaveRequest GetById(string id);
        IEnumerable<LeaveRequest> GetByUserId(string userId);
        IEnumerable<LeaveRequest> GetPendingRequests();
        void Add(LeaveRequest request);
        void Update(LeaveRequest request);
        string GenerateId();
    }

    public interface IBusinessTripRepository
    {
        BusinessTrip GetById(string id);
        IEnumerable<BusinessTrip> GetByUserId(string userId);
        IEnumerable<BusinessTrip> GetPendingRequests();
        void Add(BusinessTrip businessTrip);
        void Update(BusinessTrip businessTrip);
        string GenerateId();
    }

    public interface IPayrollRepository
    {
        Payroll GetById(string id);
        IEnumerable<Payroll> GetByUserId(string userId);
        IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end);
        void Add(Payroll payroll);
        void Update(Payroll payroll);
        string GenerateId();
    }

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private readonly object _lock = new();
        private int _nextId = 0;

        public User GetById(string id)
        {
            lock (_lock)
            {
                Log.Information($"Getting user by ID: {id}");
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    Log.Warning($"User with ID {id} not found");
                    throw new KeyNotFoundException("User not found");
                }
                return user;
            }
        }

        public User GetByEmail(string email)
        {
            lock (_lock)
            {
                Log.Information($"Getting user by email: {email}");
                var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    Log.Warning($"User with email {email} not found");
                    throw new KeyNotFoundException("User not found");
                }
                return user;
            }
        }

        public IEnumerable<User> GetAll()
        {
            lock (_lock)
            {
                Log.Information("Getting all users");
                return _users.ToList();
            }
        }

        public void Add(User user)
        {
            lock (_lock)
            {
                Log.Information($"Adding user: {user.Name}");
                if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))) {
                    Log.Error($"Email {user.Email} already exists");
                    throw new InvalidOperationException("Email already exists");
                }

                _users.Add(user);
                Log.Information($"User {user.Name} added successfully");
            }
        }

        public void Update(User user)
        {
            lock (_lock)
            {
                Log.Information($"Updating user: {user.Name}");
                var index = _users.FindIndex(u => u.Id == user.Id);
                if (index >= 0)
                {
                    _users[index] = user;
                    Log.Information($"User {user.Name} updated successfully");
                }
                else
                {
                    Log.Warning($"User with ID {user.Id} not found for update");
                    throw new KeyNotFoundException("User not found");
                }
            }
        }

        public void Delete(string id)
        {
            lock (_lock)
            {
                Log.Information($"Deleting user with ID: {id}");
                var count = _users.RemoveAll(u => u.Id == id); ;
                if (count > 0)
                {
                    Log.Warning($"User with ID {id} not found for deletion");
                    throw new KeyNotFoundException("User not found");
                }
                else
                {
                    Log.Warning($"User with ID {id} deleted successfully");
                }
            }
        }

        public bool Exists(string email)
        {
            lock (_lock)
            {
                Log.Debug($"Checking if email exists: {email}");
                return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }

        public string GenerateId()
        {
            lock (_lock)
            {
                var newId = (++_nextId).ToString();
                Log.Debug($"Generated new user ID: {newId}");
                return newId;
            }
        }
    }

    public class InMemoryLeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly List<LeaveRequest> _requests = new();
        private readonly object _lock = new();
        private int _nextId = 0;

        public LeaveRequest GetById(string id)
        {
            lock (_lock)
            {
                Log.Information($"Getting leave request by ID: {id}");
                var request = _requests.FirstOrDefault(r => r.Id == id);
                if (request == null)
                {
                    Log.Warning($"Leave request with ID {id} not found");
                    throw new KeyNotFoundException("Leave request not found");
                }
                return request;
            }
        }

        public IEnumerable<LeaveRequest> GetByUserId(string userId)
        {
            lock (_lock)
            {
                Log.Information($"Getting leave requests for user ID: {userId}");
                return _requests.Where(r => r.UserId == userId).ToList();
            }
        }

        public IEnumerable<LeaveRequest> GetPendingRequests()
        {
            lock (_lock)
            {
                Log.Information("Getting all pending leave requests");
                return _requests.Where(r => r.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(LeaveRequest request)
        {
            lock (_lock)
            {
                Log.Information($"Adding leave request for user ID: {request.UserId}");
                _requests.Add(request);
                Log.Debug($"Leave request for user ID {request.UserId} added successfully");
            }
        }

        public void Update(LeaveRequest request)
        {
            lock (_lock)
            {
                Log.Information($"Updating leave request for user ID: {request.UserId}");
                var index = _requests.FindIndex(r => r.Id == request.Id);
                if (index >= 0)
                {
                    _requests[index] = request;
                    Log.Debug($"Leave request for user ID {request.UserId} updated successfully");
                }
                else
                {
                    Log.Warning($"Leave request with ID {request.Id} not found for update");
                    throw new KeyNotFoundException("Leave request not found");
                }
            }
        }

        public string GenerateId()
        {
            lock (_lock)
            {
                var newId = (++_nextId).ToString();
                Log.Debug($"Generated new leave request ID: {newId}");
                return newId;
            }
        }
    }

    public class InMemoryBusinessTripRepository : IBusinessTripRepository
    {
        private readonly List<BusinessTrip> _trips = new();
        private readonly object _lock = new();
        private int _nextId = 0;

        public BusinessTrip GetById(string id)
        {
            lock (_lock)
            {
                Log.Information($"Getting business trip by ID: {id}");
                var trip = _trips.FirstOrDefault(t => t.Id == id);
                if (trip == null)
                {
                    Log.Warning($"Business trip with ID {id} not found");
                    throw new KeyNotFoundException("Business trip not found");
                }
                return trip;
            }
        }

        public IEnumerable<BusinessTrip> GetByUserId(string userId)
        {
            lock (_lock)
            {
                Log.Information($"Getting business trips for user ID: {userId}");
                return _trips.Where(t => t.UserId == userId).ToList();
            }
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            lock (_lock)
            {
                Log.Information("Getting all pending business trip requests");
                return _trips.Where(t => t.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(BusinessTrip businessTrip)
        {
            lock (_lock)
            {
                Log.Information($"Adding business trip for user ID: {businessTrip.UserId}");
                _trips.Add(businessTrip);
                Log.Debug($"Business trip for user ID {businessTrip.UserId} added successfully");
            }
        }

        public void Update(BusinessTrip businessTrip)
        {
            lock (_lock)
            {
                Log.Information($"Updating business trip for user ID: {businessTrip.UserId}");
                var index = _trips.FindIndex(t => t.Id == businessTrip.Id);
                if (index >= 0)
                {
                    _trips[index] = businessTrip;
                    Log.Information($"Business trip for user ID {businessTrip.UserId} updated successfully");
                }
                else
                {
                    Log.Warning($"Business trip with ID {businessTrip.Id} not found for update");
                    throw new KeyNotFoundException("Business trip not found");
                }
            }
        }

        public string GenerateId()
        {
            lock (_lock)
            {
                var newId = (++_nextId).ToString();
                Log.Debug($"Generated new business trip ID: {newId}");
                return newId;
            }
        }
    }

    public class InMemoryPayrollRepository : IPayrollRepository
    {
        private readonly List<Payroll> _payrolls = new();
        private readonly object _lock = new();
        private int _nextId = 0;

        public Payroll GetById(string id)
        {
            lock (_lock)
            {
                Log.Information($"Getting payroll by ID: {id}");
                var payroll = _payrolls.FirstOrDefault(p => p.Id == id);
                if (payroll == null)
                {
                    Log.Warning($"Payroll with ID {id} not found");
                    throw new KeyNotFoundException("Payroll not found");
                }
                return payroll;
            }
        }

        public IEnumerable<Payroll> GetByUserId(string userId)
        {
            lock (_lock)
            {
                Log.Information($"Getting payrolls for user ID: {userId}");
                return _payrolls.Where(p => p.UserId == userId).ToList();
            }
        }

        public IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end)
        {
            lock (_lock)
            {
                Log.Information($"Getting payrolls between {start} and {end}");
                return _payrolls.Where(p => p.PeriodStart >= start && p.PeriodEnd <= end).ToList();
            }
        }

        public void Add(Payroll payroll)
        {
            lock (_lock)
            {
                Log.Information($"Adding payroll for user ID: {payroll.UserId}");
                _payrolls.Add(payroll);
                Log.Debug($"Payroll for user ID {payroll.UserId} added successfully");
            }
        }

        public void Update(Payroll payroll)
        {
            lock (_lock)
            {
                Log.Information($"Updating payroll for user ID: {payroll.UserId}");
                var index = _payrolls.FindIndex(p => p.Id == payroll.Id);
                if (index >= 0)
                {
                    _payrolls[index] = payroll;
                    Log.Information($"Payroll for user ID {payroll.UserId} updated successfully");
                }
                else
                {
                    Log.Warning($"Payroll with ID {payroll.Id} not found for update");
                    throw new KeyNotFoundException("Payroll not found");
                }
            }
        }
        public string GenerateId()
        {
            lock (_lock)
            {
                var newId = (++_nextId).ToString();
                Log.Debug($"Generated new payroll ID: {newId}");
                return newId;
            }
        }
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger _logger;

        public UserService(IUserRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger.ForContext<UserService>();
            _logger = logger;
        }

        public User Register(string name, string email, string password, Role role)
        {
            _logger.Information($"Registering user: {name}, Email: {email}, Role: {role}");
            if (!IsValidEmail(email))
            {
                _logger.Error($"Invalid email format: {email}");
                throw new ArgumentException("Invalid email format");
            }
            if (_repository.Exists(email))
            {
                _logger.Error($"Email already registered: {email}");
                throw new ArgumentException("Email already registered");
            }
            
            if (password.Length < 8)
            {
                _logger.Error($"Password too short: {password.Length} characters");
                throw new ArgumentException("Password must be at least 8 characters");
            }

            var user = new User
            {
                Id = _repository.GenerateId().ToString(),
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                RemainingLeaveDays = 12,
                IsActive = true,
                JoinDate = DateTime.Now
            };

            _repository.Add(user);
            _logger.Information($"User {name} registered successfully, {role}");
            return user;
        }

        public User? Authenticate(string email, string password)
        {
            try
            {
                var user = _repository.GetByEmail(email);
                if (user.Password == password)
                {
                    _logger.Information($"User {user.Name} authenticated successfully");
                    return user;
                }
                _logger.Warning($"Invalid credentials for email: {email}");

                return null;
            }
            catch (KeyNotFoundException)
            {
                _logger.Warning($"User with email {email} not found");
                return null;
            }
        }

        public User? GetAllUser()
        {
            _logger.Debug("Getting all active users");
            return _repository.GetAll().FirstOrDefault(u => u.IsActive);
        }

        public User GetUserById(string userId)
        {
            _logger.Debug($"Getting user by ID: {userId}");
            return _repository.GetById(userId);
        }

        public void UpdateUserDetails(string userId, string name, string email)
        {
            _logger.Information($"Updating user details for ID: {userId}, Name: {name}, Email: {email}");
            if (!IsValidEmail(email))
            {
                _logger.Error($"Invalid email format: {email}");
                throw new ArgumentException("Invalid email format");
            }

            var user = _repository.GetById(userId);
            user.Name = name;
            user.Email = email;
            _repository.Update(user);
            _logger.Information($"User details updated successfully for ID: {userId}");
        }

        public void ChangePassword(string userId, string currentPassword, string newPassword)
        {
            _logger.Information($"Changing password for user ID: {userId}");
            if (newPassword.Length < 8)
            {
                _logger.Error($"New password too short: {newPassword.Length} characters");
                throw new ArgumentException("Password must be at least 8 characters");
            }

            var user = _repository.GetById(userId);
            if (user.Password != currentPassword)
            {
                _logger.Error($"Current password does not match for user ID: {userId}");
                throw new ArgumentException("Current password is incorrect");
            }
            
            user.Password = newPassword;
            _repository.Update(user);
            _logger.Information($"Password changed successfully for user ID: {userId}");
        }

        public void DeactivateUser(string userId)
        {
            _logger.Information($"Deactivating user with ID: {userId}");
            var user = _repository.GetById(userId);
            user.IsActive = false;
            _repository.Update(user);
            _logger.Information($"User with ID {userId} deactivated successfully");
        }

        public void PromoteUser(string userId, Role newRole)
        {
            _logger.Information($"Promoting user with ID: {userId} to role: {newRole}");
            var user = _repository.GetById(userId);
            user.Role = newRole;
            _repository.Update(user);
            _logger.Information($"User with ID {userId} promoted to role: {newRole}");
        }

        public void ResetPassword(string userId, string newPassword)
        {
            if (newPassword.Length < 8)
            {
                _logger.Error($"New password too short: {newPassword.Length} characters");
                throw new ArgumentException("Password must be at least 8 characters");
            }

            var user = _repository.GetById(userId);
            user.Password = newPassword;
            _repository.Update(user);
            _logger.Information($"Password reset successfully for user ID: {userId}");
        }

        public void UpdateUser(User user)
        {
            _logger.Information($"Updating user: {user.Name}");
            _repository.Update(user);
            _logger.Information($"User {user.Name} updated successfully");
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }

    public class LeaveService
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

        public LeaveRequest SubmitRequest(string userId, DateTime startDate, DateTime endDate, string description)
        {
            _logger.Information($"Submitting leave request for user ID: {userId}, Start Date: {startDate}, End Date: {endDate}");
            if (startDate < DateTime.Today)
            {
                _logger.Error($"Start date cannot be in the past: {startDate}");
                throw new ArgumentException("Start date cannot be in the past");
            }
            if (startDate > endDate)
            {
                _logger.Error($"End date must be after start date: {endDate}");
                throw new ArgumentException("End date must be after start date");
            }

            var user = _userService.GetUserById(userId);

            var duration = (endDate - startDate).Days + 1;
            if (user.RemainingLeaveDays < duration)
            {
                _logger.Error($"Insufficient leave days for user ID: {userId}, Remaining: {user.RemainingLeaveDays}, Requested: {duration}");
                throw new ArgumentException("Insufficient leave days");
            }

            var request = new LeaveRequest
            {
                Id = _leaveRepository.GenerateId().ToString(),
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending
            };

            _leaveRepository.Add(request);
            _logger.Information($"Leave request submitted successfully for user ID: {userId}");
            return request;
        }

        public void ApproveRequest(string requestId, string approverId)
        {
            _logger.Information($"Approving leave request ID: {requestId} by approver ID: {approverId}");
            var request = _leaveRepository.GetById(requestId);
            var user = _userService.GetUserById(request.UserId);

            var duration = request.Duration;
            user.RemainingLeaveDays -= duration;

            request.Status = RequestStatus.Approved;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;

            _leaveRepository.Update(request);
            _userService.UpdateUser(user);
            _logger.Information($"Leave request ID: {requestId} approved successfully by approver ID: {approverId}");
        }

        public void RejectRequest(string requestId, string approverId, string reason)
        {
            _logger.Information($"Rejecting leave request ID: {requestId} by approver ID: {approverId}");
            if (string.IsNullOrWhiteSpace(reason))
            {
                _logger.Error("Rejection reason is required");
                throw new ArgumentException("Rejection reason is required");
            }

            var request = _leaveRepository.GetById(requestId);
            request.Status = RequestStatus.Rejected;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;
            request.RejectionReason = reason;

            _leaveRepository.Update(request);
            _logger.Information($"Leave request ID: {requestId} rejected successfully by approver ID: {approverId}");
        }

        public IEnumerable<LeaveRequest> GetPendingRequests()
        {
            _logger.Debug("Getting all pending leave requests");
            return _leaveRepository.GetPendingRequests();
        }

        public IEnumerable<LeaveRequest> GetByUserId(string userId)
        {
            _logger.Debug($"Getting leave requests for user ID: {userId}");
            return _leaveRepository.GetByUserId(userId);
        }
    }

    public class BusinessTripService
    {
        private readonly IBusinessTripRepository _tripRepository;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public BusinessTripService(IBusinessTripRepository tripRepository, IUserService userService, ILogger logger)
        {
            _tripRepository = tripRepository;
            _userService = userService;
            _logger = logger.ForContext<BusinessTrip>();
        }

        public BusinessTrip SubmitRequest(string userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost)
        {
            if (startDate < DateTime.Today)
            {
                _logger.Error($"Start date cannot be in the past: {startDate}");
                throw new ArgumentException("Start date cannot be in the past");
            }
            if (startDate > endDate)
            {
                _logger.Error($"End date must be after start date: {endDate}");
                throw new ArgumentException("End date must be after start date");
            }
            if (estimateCost <= 0)
            {
                _logger.Error($"Estimated cost must be greater than zero: {estimateCost}");
                throw new ArgumentException("Estimated cost must be greater than zero");
            }

            var user = _userService.GetUserById(userId);

            var trip = new BusinessTrip
            {
                Id = _tripRepository.GenerateId().ToString(),
                UserId = userId,
                Destination = destination,
                StartDate = startDate,
                EndDate = endDate,
                Purpose = purpose,
                EstimatedCost = estimateCost,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending
            };

            _tripRepository.Add(trip);
            _logger.Information($"Business trip request submitted successfully for user ID: {userId}");
            return trip;
        }

        public void ApproveRequest(string tripId, string approverId)
        {
            _logger.Information($"Approving business trip request ID: {tripId} by approver ID: {approverId}");
            var trip = _tripRepository.GetById(tripId);
            trip.Status = RequestStatus.Approved;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;
            _tripRepository.Update(trip);
            _logger.Information($"Business trip request ID: {tripId} approved successfully by approver ID: {approverId}");
        }

        public void RejectRequest(string tripId, string approverId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                _logger.Error("Rejection reason is required");
                throw new ArgumentException("Rejection reason is required");
            }

            var trip = _tripRepository.GetById(tripId);
            trip.Status = RequestStatus.Rejected;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;
            trip.RejectionReason = reason;
            _tripRepository.Update(trip);
            _logger.Information($"Business trip request ID: {tripId} rejected successfully by approver ID: {approverId}");
        }

        public void UpdateActualCost(string tripId, decimal actualCost)
        {
            _logger.Information($"Updating actual cost for business trip ID: {tripId}, Actual Cost: {actualCost}");
            if (actualCost < 0)
            {
                _logger.Error($"Actual cost cannot be negative: {actualCost}");
                throw new ArgumentException("Actual cost cannot be negative");
            }

            var trip = _tripRepository.GetById(tripId);
            trip.ActualCost = actualCost;
            _tripRepository.Update(trip);
            _logger.Information($"Actual cost updated successfully for business trip ID: {tripId}");
        }

        public IEnumerable<BusinessTrip> GetByUserId(string userId)
        {
            _logger.Debug($"Getting business trips for user ID: {userId}");
            return _tripRepository.GetByUserId(userId);
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            _logger.Debug("Getting all pending business trip requests");
            return _tripRepository.GetPendingRequests();
        }
    }

    public class PayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IUserService _userService;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IBusinessTripRepository _businessTripRepository;
        private readonly ILogger _logger;

        public PayrollService(
            IPayrollRepository payrollRepository,
            IUserService userService,
            ILeaveRequestRepository leaveRequestRepository,
            IBusinessTripRepository businessTripRepository,
            ILogger logger)
        {
            _payrollRepository = payrollRepository;
            _userService = userService;
            _leaveRequestRepository = leaveRequestRepository;
            _businessTripRepository = businessTripRepository;
            _logger = logger.ForContext<PayrollService>();
        }

        public Payroll GeneratePayroll(string userId, DateTime periodStart, DateTime periodEnd)
        {
            _logger.Information($"Generating payroll for user ID: {userId}, Period: {periodStart} to {periodEnd}");
            if (periodStart >= periodEnd)
            {
                _logger.Error($"End date must be after start date: {periodEnd}");
                throw new ArgumentException("End date must be after start date");
            }

            var user = _userService.GetUserById(userId);

            var trips = _businessTripRepository.GetByUserId(userId)
                .Where(t => t.Status == RequestStatus.Approved &&
                          t.StartDate >= periodStart &&
                          t.EndDate <= periodEnd);

            decimal travelAllowance = trips.Sum(t => t.EstimatedCost * 0.1m);

            var payroll = new Payroll
            {
                Id = _payrollRepository.GenerateId().ToString(),
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                TravelAllowance = travelAllowance,
                PaymentDate = DateTime.Now.AddDays(7),
                IsPaid = false
            };

            _payrollRepository.Add(payroll);
            _logger.Information($"Payroll generated successfully for user ID: {userId}, Period: {periodStart} to {periodEnd}");
            return payroll;
        }

        public void MarkAsPaid(string payrollId)
        {
            _logger.Information($"Marking payroll ID: {payrollId} as paid");
            var payroll = _payrollRepository.GetById(payrollId);
            payroll.IsPaid = true;
            payroll.PaymentDate = DateTime.Now;
            _payrollRepository.Update(payroll);
            _logger.Information($"Payroll ID: {payrollId} marked as paid successfully");
        }

        public IEnumerable<Payroll> GetByUserId(string userId)
        {
            _logger.Debug($"Getting payrolls for user ID: {userId}");
            return _payrollRepository.GetByUserId(userId);
        }

        public IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end)
        {
            _logger.Debug($"Getting payrolls between {start} and {end}");
            if (start > end)
            {
                _logger.Error($"End date must be after start date: {end}");
                throw new ArgumentException("End date must be after start date");
            }
            return _payrollRepository.GetByPeriod(start, end);
        }
    }

    public static class RoleExtensions
    {
        public static bool CanManageUsers(this Role role) => role >= Role.HRD;
        public static bool CanApproveLeave(this Role role) => role >= Role.Supervisor;
        public static bool CanApproveTrips(this Role role) => role >= Role.Supervisor;
        public static bool CanManagePayroll(this Role role) => role >= Role.Finance;
        public static bool CanManageSystem(this Role role) => role == Role.SysAdmin;
    }

    public static class LoggerConfig
    {
        public static ILogger ConfigureLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/system.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true)
                .CreateLogger();
        }
    }
}