namespace CoreLibrary.ModelLib
{
    public class LeaveRequest
    {
        public required int Id { get; set; }
        public required int UserId { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public int ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.Now;
        public string RejectionReason { get; set; } = string.Empty;
        public int Duration => (EndDate - StartDate).Days + 1;
    }
}
