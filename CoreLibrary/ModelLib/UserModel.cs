namespace CoreLibrary.ModelLib
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public Role Role { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public int RemainingLeaveDays { get; set; } = 12;
        public decimal BasicSalary { get; set; } = decimal.Zero;
    }
}   
