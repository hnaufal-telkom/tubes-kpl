using Serilog;
namespace CoreLibrary
{
    public enum RequestStatus { Pending, Approved, Rejected, Cancelled }
    public enum Role { Employee, HRD, Supervisor, Finance, SysAdmin }

    public static class RoleExtensions
    {
        public static bool CanManageUsers(this Role role) => role == Role.HRD || role == Role.SysAdmin;
        public static bool CanApproveLeave(this Role role) => role == Role.Supervisor || role == Role.SysAdmin;
        public static bool CanApproveTrips(this Role role) => role == Role.Supervisor || role == Role.SysAdmin;
        public static bool CanManagePayroll(this Role role) => role == Role.Finance || role == Role.SysAdmin;
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
