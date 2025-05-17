namespace CoreLibrary.ModelLib
{
    public class Payroll
    {
        public required int Id { get; set; }
        public required int UserId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal BasicSalary { get; set; } = decimal.Zero;
        public bool IsPaid { get; set; } = false;
        public DateTime PaymentDate { get; set; } = DateTime.Now.AddDays(7);
    }
}
