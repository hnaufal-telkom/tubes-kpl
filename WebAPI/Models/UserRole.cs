namespace WebAPI.Models
{
    public static class UserRole
    {
        public const string Employee = "employee";
        public const string Supervisor = "supervisor";
        public const string HRD = "hrd";
        public const string Finance = "finance";
        public const string SysAdmin = "sysadmin";
        public static string[] validRoles()
        {
            return new[]
            {
                Employee,
                Supervisor,
                HRD,
                Finance,
                SysAdmin
            };
        }
    }
}
