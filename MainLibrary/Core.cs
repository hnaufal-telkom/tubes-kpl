﻿using System.Text.Json;
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
        public decimal ActualCost { get; set; }
        public string? ApproverId { get; set; }
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

    public interface IUserService
    {
        User GetUserById(string id);
        IEnumerable<User> GetAllUsers();
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(string id);
        bool Exists(string email);
    }

    public interface ILeaveRequestRepository
    {
        LeaveRequest GetLeaveById(string id);
        IEnumerable<LeaveRequest> GetByUserId(string userId);
        IEnumerable<LeaveRequest> GetPendingRequest();
        void AddPendingRequest(LeaveRequest request);
        void UpdatePendingRequest(LeaveRequest request);
    }

    public interface IBusinessTripRepository
    {
        BusinessTrip GetBusinessTripById(string id);
        IEnumerable<BusinessTrip> GetBusinessTripByUserId(string userId);
        IEnumerable<BusinessTrip> GetPendingBusinessRequest();
        void Add(BusinessTrip businessTrip);
        void Update(BusinessTrip businessTrip);
    }

    public interface IPayrollRepository
    {
        Payroll GetPayrollById(string id);
        IEnumerable<Payroll> GetPayrollByUserId(string userId);
        IEnumerable<Payroll> GeyPayrollByPeriod(DateTime start, DateTime end);
        void Add(Payroll payroll);
        void Update(Payroll payroll);
    }

    public class UserService
    {
        private readonly IUserService _repository;

        public UserService(IUserService repository)
        {
            _repository = repository;
        }

        public User Register(string name, string email, string password, Role role)
        {
            if (_repository.Equals(email)) throw new InvalidOperationException("Email alredy registered");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                JoinDate = DateTime.Now
            };

            _repository.AddUser(user);
            return user;
        }

        public User Authenticate(string email, string password)
        {
            var user = _repository.GetAllUsers().FirstOrDefault(x => x.Email == email);
            if (user == null || !password.Equals(user.Password)) return null;
            return user!;
        }

        public void UpdateUserDetails(string userId, string name, string email)
        {
            var user = _repository.GetUserById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Name = name;
            user.Email = email;
            _repository.UpdateUser(user);
        }

        public void ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var user = _repository.GetUserById(userId);
            if (user == null || !currentPassword.Equals(user.Password)) throw new UnauthorizedAccessException("Invalid credentials");
            user.Password = newPassword;
            _repository.UpdateUser(user);
        }

        public void DeactivateUser(string userId)
        {
            var user = _repository.GetUserById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.IsActive = false;
            _repository.UpdateUser(user);
        }
    }

    public class LeaveService
    {
        private readonly ILeaveRequestRepository _leaveRepository;
        private readonly IUserService _userRepository;

        public LeaveService(ILeaveRequestRepository leaveRepository, IUserService userRepository)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
        }

        public LeaveRequest SubmitRequest(string userId, DateTime startDate, DateTime endDate, string description)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            if (startDate > endDate) throw new ArgumentException("End date must be after start date");

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

            _leaveRepository.AddPendingRequest(request);
            return request;
        }

        public void ApproveRequest(string requestId, string approverId)
        {
            var request = _leaveRepository.GetLeaveById(requestId);
            if (request == null) throw new KeyNotFoundException("Request not found");

            var user = _userRepository.GetUserById(request.UserId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var duration = request.Duration;
            user.RemainingLeaveDays -= duration;

            request.Status = RequestStatus.Approved;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;

            _leaveRepository.UpdatePendingRequest(request);
            _userRepository.UpdateUser(user);
        }

        public void RejectRequest(string requestId, string approverId, string reason)
        {
            var request = _leaveRepository.GetLeaveById(requestId);
            if (request == null) throw new KeyNotFoundException("Request not found");

            request.Status = RequestStatus.Rejected;
            request.ApproverId = approverId;
            request.ApprovalDate = DateTime.Now;
            request.RejectionReason = reason;

            _leaveRepository.UpdatePendingRequest(request);
        }
    }

    public class BusinessTripService
    {
        private readonly IBusinessTripRepository _tripRepository;
        private readonly IUserService _userRepository;

        public BusinessTripService(IBusinessTripRepository tripRepository, IUserService userRepository)
        {
            _tripRepository = tripRepository;
            _userRepository = userRepository;
        }

        public BusinessTrip SubmitRequest(string userId, string destination, DateTime startDate, DateTime endDate, string purpose, decimal estimateCost)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            if (startDate > endDate) throw new ArgumentException("End date must be after start date");

            var trip = new BusinessTrip
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Destination = destination,
                ApprovalDate = startDate,
                EndDate = endDate,
                Purpose = purpose,
                EstimatedCost = estimateCost,
                Status = RequestStatus.Pending
            };

            _tripRepository.Add(trip);
            return trip;
        }

        public void ApproveRequest(string tripId, string approverId)
        {
            var trip = _tripRepository.GetBusinessTripById(tripId);
            if (trip == null) throw new KeyNotFoundException("Trip request not found");

            trip.Status = RequestStatus.Approved;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;

            _tripRepository.Update(trip);
        }

        public void RejectRequest(string tripId, string approverId)
        {
            var trip = _tripRepository.GetBusinessTripById(tripId);
            if (trip == null) throw new KeyNotFoundException("Trip request not found");

            trip.Status = RequestStatus.Rejected;
            trip.ApproverId = approverId;
            trip.ApprovalDate = DateTime.Now;

            _tripRepository.Update(trip);
        }

        public void UpdateActualCost(string tripId, decimal actualCost)
        {
            var trip = _tripRepository.GetBusinessTripById(tripId);
            if (trip == null) throw new KeyNotFoundException("Trip request not found");

            trip.ActualCost = actualCost;
            _tripRepository.Update(trip);
        }

        public IEnumerable<BusinessTrip> GetBusinessTripByUserId(string userId)
        {
            return _tripRepository.GetBusinessTripByUserId(userId);
        }

        public IEnumerable<BusinessTrip> GetPendingBusinessRequest()
        {
            return _tripRepository.GetPendingBusinessRequest();
        }
    }

    public class PayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IUserService _userService;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IBusinessTripRepository _businessTripRepository;

        public PayrollService(IPayrollRepository payrollRepository, IUserService userService, ILeaveRequestRepository leaveRequestRepository, IBusinessTripRepository businessTripRepository)
        {
            _payrollRepository = payrollRepository;
            _userService = userService;
            _leaveRequestRepository = leaveRequestRepository;
            _businessTripRepository = businessTripRepository;
        }

        public Payroll GeneratePayroll(string userId, DateTime periodStart, DateTime periodEnd)
        {
            var user = _userService.GetUserById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var leaves = _leaveRequestRepository.GetByUserId(userId)
                .Where(l => l.Status == RequestStatus.Approved &&
                          l.StartDate >= periodStart &&
                          l.EndDate <= periodEnd);

            var trips = _businessTripRepository.GetBusinessTripByUserId(userId)
                .Where(t => t.Status == RequestStatus.Approved &&
                          t.StartDate >= periodStart &&
                          t.EndDate <= periodEnd);


            decimal travelAllowance = 0;
            foreach (var trip in trips)
            {
                travelAllowance += trip.EstimatedCost * 0.1m;
            }

            var payroll = new Payroll
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
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
            var payroll = _payrollRepository.GetPayrollById(payrollId);
            if (payroll == null) throw new KeyNotFoundException("Payroll not found");

            payroll.IsPaid = true;
            payroll.PaymentDate = DateTime.Now;
            _payrollRepository.Update(payroll);
        }

        public IEnumerable<Payroll> GetUserPayrolls(string userId)
        {
            return _payrollRepository.GetPayrollByUserId(userId);
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