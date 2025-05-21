using CoreLibrary;

namespace WebAPI.DTO
{ 
    public class BusinessTripDTO
    {
        public required int UserId { get; set; }
        public required string Destination { get; set; } = string.Empty;
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string Purpose { get; set; } = string.Empty;
        public required decimal EstimateCost { get; set; } = decimal.Zero;
    }

    public class PayrollDTO
    {
        public required DateTime PeriodStart { get; set; }
        public required DateTime PeriodEnd { get; set; }    
    }
}
