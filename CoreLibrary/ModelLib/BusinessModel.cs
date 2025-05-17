namespace CoreLibrary.ModelLib
{
    public class BusinessTrip
    {
        public required int Id { get; set; }
        public required int UserId { get; set; }
        public required string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string Purpose { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public decimal EstimatedCost { get; set; } = 0;
        public decimal ActualCost { get; set; } = 0;
        public int ApproverId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public required DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime ApprovalDate {  get; set; }
    }
}
