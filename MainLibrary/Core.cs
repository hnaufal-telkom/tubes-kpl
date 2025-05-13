using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MainLibrary
{
    public enum RequestStatus { Pending, Approved, Rejected, Cancelled }
    public enum Role { Employee, HRD, Supervisor, Finance, SysAdmin }

    public class User
    {
        public required string Id { get; set; }
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
        public required string Id { get; set; }
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
        public required string Id { get; set; }
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
        public required string Id { get; set; }
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
    }

    public interface IUserService
    {
        User Register(string name, string email, string password, Role role);
        User? Authenticate(string email, string password);
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
    }

    public interface IBusinessTripRepository
    {
        BusinessTrip GetById(string id);
        IEnumerable<BusinessTrip> GetByUserId(string userId);
        IEnumerable<BusinessTrip> GetPendingRequests();
        void Add(BusinessTrip businessTrip);
        void Update(BusinessTrip businessTrip);
    }

    public interface IPayrollRepository
    {
        Payroll GetById(string id);
        IEnumerable<Payroll> GetByUserId(string userId);
        IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end);
        void Add(Payroll payroll);
        void Update(Payroll payroll);
    }

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private readonly object _lock = new();

        public User GetById(string id)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u => u.Id == id) ?? throw new KeyNotFoundException("User not found");
            }
        }

        public User GetByEmail(string email)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ?? throw new KeyNotFoundException("User not found");
            }
        }

        public IEnumerable<User> GetAll()
        {
            lock (_lock)
            {
                return _users.ToList();
            }
        }

        public void Add(User user)
        {
            lock (_lock)
            {
                if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Email already exists");

                _users.Add(user);
            }
        }

        public void Update(User user)
        {
            lock (_lock)
            {
                var index = _users.FindIndex(u => u.Id == user.Id);
                if (index >= 0) _users[index] = user;
            }
        }

        public void Delete(string id)
        {
            lock (_lock)
            {
                _users.RemoveAll(u => u.Id == id);
            }
        }

        public bool Exists(string email)
        {
            lock (_lock)
            {
                return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    public class InMemoryLeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly List<LeaveRequest> _requests = new();
        private readonly object _lock = new();

        public LeaveRequest GetById(string id)
        {
            lock (_lock)
            {
                return _requests.FirstOrDefault(r => r.Id == id) ?? throw new KeyNotFoundException("Leave request not found");
            }
        }

        public IEnumerable<LeaveRequest> GetByUserId(string userId)
        {
            lock (_lock)
            {
                return _requests.Where(r => r.UserId == userId).ToList();
            }
        }

        public IEnumerable<LeaveRequest> GetPendingRequests()
        {
            lock (_lock)
            {
                return _requests.Where(r => r.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(LeaveRequest request)
        {
            lock (_lock)
            {
                _requests.Add(request);
            }
        }

        public void Update(LeaveRequest request)
        {
            lock (_lock)
            {
                var index = _requests.FindIndex(r => r.Id == request.Id);
                if (index >= 0) _requests[index] = request;
            }
        }
    }

    public class InMemoryBusinessTripRepository : IBusinessTripRepository
    {
        private readonly List<BusinessTrip> _trips = new();
        private readonly object _lock = new();

        public BusinessTrip GetById(string id)
        {
            lock (_lock)
            {
                return _trips.FirstOrDefault(t => t.Id == id) ?? throw new KeyNotFoundException("Business trip not found");
            }
        }

        public IEnumerable<BusinessTrip> GetByUserId(string userId)
        {
            lock (_lock)
            {
                return _trips.Where(t => t.UserId == userId).ToList();
            }
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            lock (_lock)
            {
                return _trips.Where(t => t.Status == RequestStatus.Pending).ToList();
            }
        }

        public void Add(BusinessTrip businessTrip)
        {
            lock (_lock)
            {
                _trips.Add(businessTrip);
            }
        }

        public void Update(BusinessTrip businessTrip)
        {
            lock (_lock)
            {
                var index = _trips.FindIndex(t => t.Id == businessTrip.Id);
                if (index >= 0) _trips[index] = businessTrip;
            }
        }
    }

    public class InMemoryPayrollRepository : IPayrollRepository
    {
        private readonly List<Payroll> _payrolls = new();
        private readonly object _lock = new();

        public Payroll GetById(string id)
        {
            lock (_lock)
            {
                return _payrolls.FirstOrDefault(p => p.Id == id) ?? throw new KeyNotFoundException("Payroll not found");
            }
        }

        public IEnumerable<Payroll> GetByUserId(string userId)
        {
            lock (_lock)
            {
                return _payrolls.Where(p => p.UserId == userId).ToList();
            }
        }

        public IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end)
        {
            lock (_lock)
            {
                return _payrolls.Where(p => p.PeriodStart >= start && p.PeriodEnd <= end).ToList();
            }
        }

        public void Add(Payroll payroll)
        {
            lock (_lock)
            {
                _payrolls.Add(payroll);
            }
        }

        public void Update(Payroll payroll)
        {
            lock (_lock)
            {
                var index = _payrolls.FindIndex(p => p.Id == payroll.Id);
                if (index >= 0) _payrolls[index] = payroll;
            }
        }
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public User Register(string name, string email, string password, Role role)
        {
            if (!IsValidEmail(email)) throw new ArgumentException("Invalid email format");
            if (_repository.Exists(email)) throw new ArgumentException("Email already registered");
            if (password.Length < 8) throw new ArgumentException("Password must be at least 8 characters");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                JoinDate = DateTime.Now
            };

            _repository.Add(user);
            return user;
        }

        public User? Authenticate(string email, string password)
        {
            try
            {
                var user = _repository.GetByEmail(email);
                return user.Password == password ? user : null;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public User GetUserById(string userId)
        {
            return _repository.GetById(userId);
        }

        public void UpdateUserDetails(string userId, string name, string email)
        {
            if (!IsValidEmail(email)) throw new ArgumentException("Invalid email format");

            var user = _repository.GetById(userId);
            user.Name = name;
            user.Email = email;
            _repository.Update(user);
        }

        public void ChangePassword(string userId, string currentPassword, string newPassword)
        {
            if (newPassword.Length < 8) throw new ArgumentException("Password must be at least 8 characters");

            var user = _repository.GetById(userId);
            if (user.Password != currentPassword)
                throw new UnauthorizedAccessException("Invalid credentials");

            user.Password = newPassword;
            _repository.Update(user);
        }

        public void DeactivateUser(string userId)
        {
            var user = _repository.GetById(userId);
            user.IsActive = false;
            _repository.Update(user);
        }

        public void PromoteUser(string userId, Role newRole)
        {
            var user = _repository.GetById(userId);
            user.Role = newRole;
            _repository.Update(user);
        }

        public void ResetPassword(string userId, string newPassword)
        {
            if (newPassword.Length < 8) throw new ArgumentException("Password must be at least 8 characters");

            var user = _repository.GetById(userId);
            user.Password = newPassword;
            _repository.Update(user);
        }

        public void UpdateUser(User user)
        {
            _repository.Update(user);
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

        public LeaveService(ILeaveRequestRepository leaveRepository, IUserService userService)
        {
            _leaveRepository = leaveRepository;
            _userService = userService;
        }

        public LeaveRequest SubmitRequest(string userId, DateTime startDate, DateTime endDate, string description)
        {
            if (startDate < DateTime.Today) throw new ArgumentException("Start date cannot be in the past");
            if (startDate > endDate) throw new ArgumentException("End date must be after start date");

            var user = _userService.GetUserById(userId);

            var duration = (endDate - startDate).Days + 1;
            if (user.RemainingLeaveDays < duration) throw new InvalidOperationException("Not enough remaining leave days");

            var request = new LeaveRequest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending
            };

            _leaveRepository.Add(request);
            return request;
        }

        public void ApproveRequest(string requestId, string approverId)
        {
            var request = _leaveRepository.GetById(requestId);
            var user = _userService.GetUserById(request.UserId);

            var duration = request.Duration;
            user.RemainingLeaveDays -= duration;

            request.Status = RequestStatus.Approved;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;

            _leaveRepository.Update(request);
            _userService.UpdateUser(user);
        }

        public void RejectRequest(string requestId, string approverId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Rejection reason is required");

            var request = _leaveRepository.GetById(requestId);
            request.Status = RequestStatus.Rejected;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;
            request.RejectionReason = reason;

            _leaveRepository.Update(request);
        }

        public IEnumerable<LeaveRequest> GetPendingRequests()
        {
            return _leaveRepository.GetPendingRequests();
        }

        public IEnumerable<LeaveRequest> GetByUserId(string userId)
        {
            return _leaveRepository.GetByUserId(userId);
        }
    }

    public class BusinessTripService
    {
        private readonly IBusinessTripRepository _tripRepository;
        private readonly IUserService _userService;

        public BusinessTripService(IBusinessTripRepository tripRepository, IUserService userService)
        {
            _tripRepository = tripRepository;
            _userService = userService;
        }

        public BusinessTrip SubmitRequest(string userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost)
        {
            if (startDate < DateTime.Today) throw new ArgumentException("Start date cannot be in the past");
            if (startDate > endDate) throw new ArgumentException("End date must be after start date");
            if (estimateCost <= 0) throw new ArgumentException("Estimated cost must be positive");

            var user = _userService.GetUserById(userId);

            var trip = new BusinessTrip
            {
                Id = Guid.NewGuid().ToString(),
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
            return trip;
        }

        public void ApproveRequest(string tripId, string approverId)
        {
            var trip = _tripRepository.GetById(tripId);
            trip.Status = RequestStatus.Approved;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;
            _tripRepository.Update(trip);
        }

        public void RejectRequest(string tripId, string approverId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Rejection reason is required");

            var trip = _tripRepository.GetById(tripId);
            trip.Status = RequestStatus.Rejected;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;
            trip.RejectionReason = reason;
            _tripRepository.Update(trip);
        }

        public void UpdateActualCost(string tripId, decimal actualCost)
        {
            if (actualCost < 0) throw new ArgumentException("Actual cost cannot be negative");

            var trip = _tripRepository.GetById(tripId);
            trip.ActualCost = actualCost;
            _tripRepository.Update(trip);
        }

        public IEnumerable<BusinessTrip> GetByUserId(string userId)
        {
            return _tripRepository.GetByUserId(userId);
        }

        public IEnumerable<BusinessTrip> GetPendingRequests()
        {
            return _tripRepository.GetPendingRequests();
        }
    }

    public class PayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IUserService _userService;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IBusinessTripRepository _businessTripRepository;

        public PayrollService(
            IPayrollRepository payrollRepository,
            IUserService userService,
            ILeaveRequestRepository leaveRequestRepository,
            IBusinessTripRepository businessTripRepository)
        {
            _payrollRepository = payrollRepository;
            _userService = userService;
            _leaveRequestRepository = leaveRequestRepository;
            _businessTripRepository = businessTripRepository;
        }

        public Payroll GeneratePayroll(string userId, DateTime periodStart, DateTime periodEnd)
        {
            if (periodStart >= periodEnd) throw new ArgumentException("Period end must be after period start");

            var user = _userService.GetUserById(userId);

            var trips = _businessTripRepository.GetByUserId(userId)
                .Where(t => t.Status == RequestStatus.Approved &&
                          t.StartDate >= periodStart &&
                          t.EndDate <= periodEnd);

            decimal travelAllowance = trips.Sum(t => t.EstimatedCost * 0.1m);

            var payroll = new Payroll
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                BasicSalary = CalculateBasicSalary(user),
                TravelAllowance = travelAllowance,
                MealAllowance = 500000,
                PaymentDate = DateTime.Now.AddDays(5),
                IsPaid = false
            };

            _payrollRepository.Add(payroll);
            return payroll;
        }

        public void MarkAsPaid(string payrollId)
        {
            var payroll = _payrollRepository.GetById(payrollId);
            payroll.IsPaid = true;
            payroll.PaymentDate = DateTime.Now;
            _payrollRepository.Update(payroll);
        }

        public IEnumerable<Payroll> GetByUserId(string userId)
        {
            return _payrollRepository.GetByUserId(userId);
        }

        public IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end)
        {
            if (start > end) throw new ArgumentException("End date must be after start date");
            return _payrollRepository.GetByPeriod(start, end);
        }

        private decimal CalculateBasicSalary(User user)
        {
            return user.Role switch
            {
                Role.Employee => 5_000_000,
                Role.Supervisor => 8_000_000,
                Role.HRD => 10_000_000,
                Role.Finance => 12_000_000,
                Role.SysAdmin => 15_000_000,
                _ => 5_000_000
            };
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
}