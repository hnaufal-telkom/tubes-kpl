using CoreLibrary;

namespace WebAPI.DTO
{ 
    public class UserDTO
    {
        public required string Name { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public required Role Role { get; set; }
        public required decimal BasicSalary { get; set; } = decimal.Zero;
    }
    public class AuthRequest
    {
        public required string Email { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }

    public class UserProfile
    {
        public required string Name { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordUser
    {
        public required string Email { get; set; } = string.Empty;
        public required string OldPassword { get; set; } = string.Empty;
        public required string NewPassword { get; set; } = string.Empty;
    }

    public class LeaveDTO
    {
        public required int UserId { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string Description { get; set; } = string.Empty;
    }

    public class BusinessTripDTO
    {
        public required int UserId { get; set; }
        public required string Destination { get; set; } = string.Empty;
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string Description { get; set; } = string.Empty;
        public required string Purpose { get; set; } = string.Empty;
        public required decimal EstimateCost { get; set; } = decimal.Zero;
    }

    public class PayrollDTO
    {
        public required DateTime PeriodStart { get; set; }
        public required DateTime PeriodEnd { get; set; }    
    }
}
