using System;
using System.Collections.Generic;
using MainLibrary;
using User = MainLibrary.User;
using LeaveRequest = MainLibrary.LeaveRequest;
using BusinessTrip = MainLibrary.BusinessTrip;

// Define the different states of the application
public enum AppState
{
    Login,
    MainMenu,
    UserManagementMenu,
    LeaveRequestMenu,
    BusinessTripMenu,
    PayrollReportMenu,
    Exit
}

public class Program
{

    private static UserService userService;
    private static LeaveService leaveService;
    private static BusinessTripService businessTripService;
    private static PayrollService payrollService;

    private static User loggedInUser = null;
    private static AppState currentState = AppState.Login;

    public static void Main(string[] args)
    {
        userService = new UserService(new MockUserRepository());
        leaveService = new LeaveService(new MockLeaveRequestRepository(), userService);
        businessTripService = new BusinessTripService(new MockBusinessTripRepository(), userService);
        payrollService = new PayrollService(new MockPayrollRepository(), userService,
                                          new MockLeaveRequestRepository(), new MockBusinessTripRepository());

        // Add some initial users for testing
        InitializeTestData();

        Console.WriteLine("Welcome to the Company Management System!");

        // Main application loop (Automata)
        while (currentState != AppState.Exit)
        {
            ProcessState();
        }

        Console.WriteLine("Thank you for using the Company Management System!");
    }
    private static void InitializeTestData()
    {
        try
        {
            // Add some test users if they don't exist
            if (!userService.Exists("admin@company.com"))
            {
                userService.Register("Admin User", "admin@company.com", "Admin123!", Role.SysAdmin);
            }
            if (!userService.Exists("hr@company.com"))
            {
                userService.Register("HR Manager", "hr@company.com", "Hr123456!", Role.HRD);
            }
            if (!userService.Exists("supervisor@company.com"))
            {
                userService.Register("Team Supervisor", "supervisor@company.com", "Super123!", Role.Supervisor);
            }
            if (!userService.Exists("finance@company.com"))
            {
                userService.Register("Finance Officer", "finance@company.com", "Finance1!", Role.Finance);
            }
            if (!userService.Exists("employee@company.com"))
            {
                userService.Register("Regular Employee", "employee@company.com", "Employee1!", Role.Employee);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing test data: {ex.Message}");
        }
    }


    private static void ProcessState()
    {
        switch (currentState)
        {
            case AppState.Login:
                ShowLogin();
                break;
            case AppState.MainMenu:
                ShowMainMenu();
                break;
            case AppState.UserManagementMenu:
                ShowUserManagementMenu();
                break;
            case AppState.LeaveRequestMenu:
                ShowLeaveRequestMenu();
                break;
            case AppState.BusinessTripMenu:
                ShowBusinessTripMenu();
                break;
            case AppState.PayrollReportMenu:
                ShowPayrollReportMenu();
                break;
        }
    }

    private static void ShowLogin()
    {
        Console.Clear();
        Console.WriteLine("\n--- Login ---");
        Console.Write("Email: ");
        string email = Console.ReadLine();
        Console.Write("Password: ");
        string password = Console.ReadLine();

        loggedInUser = userService.Authenticate(email, password);

        if (loggedInUser != null)
        {
            Console.WriteLine($"Login successful! Welcome, {loggedInUser.Name}");
            currentState = AppState.MainMenu;
        }
        else
        {
            Console.WriteLine("Invalid username or password.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine($"\n--- Main Menu ({loggedInUser.Name} - {loggedInUser.Role}) ---");
        Console.WriteLine("1. User Management");
        Console.WriteLine("2. Leave Request");
        Console.WriteLine("3. Business Trip Management");
        Console.WriteLine("4. Payroll Report");
        Console.WriteLine("5. Logout");
        Console.WriteLine("6. Exit");
        Console.Write("Enter your choice: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                if (loggedInUser.Role.CanManageUsers())
                {
                    currentState = AppState.UserManagementMenu;
                }
                else
                {
                    Console.WriteLine("You don't have permission to access this menu.");
                    Console.ReadLine();
                }
                break;
            case "2":
                currentState = AppState.LeaveRequestMenu;
                break;
            case "3":
                currentState = AppState.BusinessTripMenu;
                break;
            case "4":
                if (loggedInUser.Role.CanManagePayroll())
                {
                    currentState = AppState.PayrollReportMenu;
                }
                else
                {
                    Console.WriteLine("You don't have permission to access this menu.");
                    Console.ReadLine();
                }
                break;
            case "5":
                loggedInUser = null;
                currentState = AppState.Login;
                Console.WriteLine("Logged out successfully.");
                Console.ReadLine();
                break;
            case "6":
                currentState = AppState.Exit;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                Console.ReadLine();
                break;
        }
    }

    private static void ShowUserManagementMenu()
    {
        while (currentState == AppState.UserManagementMenu)
        {
            Console.Clear();
            Console.WriteLine("\n--- User Management ---");
            Console.WriteLine("1. View all users");
            Console.WriteLine("2. Add new user");
            Console.WriteLine("3. Edit user");
            Console.WriteLine("4. Delete user");
            Console.WriteLine("5. Back to Main Menu");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ViewAllUsers();
                    break;
                case "2":
                    AddNewUser();
                    break;
                case "3":
                    EditUser();
                    break;
                case "4":
                    DeleteUser();
                    break;
                case "5":
                    currentState = AppState.MainMenu;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }
    private static void ViewAllUsers()
    {
        Console.Clear();
        Console.WriteLine("\n--- All Users ---");
        var users = userService.GetAllUsers();

        if (users.Any())
        {
            Console.WriteLine("ID\tName\tEmail\tRole\tJoin Date\tStatus");
            Console.WriteLine("--------------------------------------------------");
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id}\t{user.Name}\t{user.Email}\t{user.Role}\t{user.JoinDate.ToShortDateString()}\t{(user.IsActive ? "Active" : "Inactive")}");
            }
        }
        else
        {
            Console.WriteLine("No users found.");
        }
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }

    private static void AddNewUser()
    {
        Console.Clear();
        Console.WriteLine("\n--- Add New User ---");

        Console.Write("Name: ");
        string name = Console.ReadLine();

        Console.Write("Email: ");
        string email = Console.ReadLine();

        Console.Write("Password: ");
        string password = Console.ReadLine();

        Console.WriteLine("Available Roles:");
        foreach (var role in Enum.GetValues(typeof(Role)))
        {
            Console.WriteLine($"{(int)role}. {role}");
        }
        Console.Write("Select Role (number): ");
        Role roleSelected = (Role)Enum.Parse(typeof(Role), Console.ReadLine());

        try
        {
            var newUser = userService.Register(name, email, password, roleSelected);
            Console.WriteLine($"User {newUser.Name} created successfully with ID: {newUser.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void EditUser()
    {
        Console.Clear();
        Console.WriteLine("\n--- Edit User ---");

        Console.Write("Enter User ID to edit: ");
        string userId = Console.ReadLine();

        try
        {
            var user = userService.GetUserById(userId);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            Console.WriteLine($"Editing user: {user.Name} ({user.Email})");

            Console.Write("New Name (leave blank to keep current): ");
            string name = Console.ReadLine();
            if (!string.IsNullOrEmpty(name)) user.Name = name;

            Console.Write("New Email (leave blank to keep current): ");
            string email = Console.ReadLine();
            if (!string.IsNullOrEmpty(email)) user.Email = email;

            userService.UpdateUser(user);
            Console.WriteLine("User updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void DeleteUser()
    {
        Console.Clear();
        Console.WriteLine("\n--- Delete User ---");

        Console.Write("Enter User ID to delete: ");
        string userId = Console.ReadLine();

        try
        {
            var user = userService.GetUserById(userId);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            Console.WriteLine($"Are you sure you want to {(user.IsActive ? "deactivate" : "activate")} user {user.Name}? (Y/N)");
            string confirm = Console.ReadLine();

            if (confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                if (user.IsActive)
                {
                    userService.DeactivateUser(userId);
                    Console.WriteLine("User deactivated successfully.");
                }
                else
                {
                    user.IsActive = true;
                    userService.UpdateUser(user);
                    Console.WriteLine("User activated successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowLeaveRequestMenu()
    {
        while (currentState == AppState.LeaveRequestMenu)
        {
            Console.Clear();
            Console.WriteLine("\n--- Leave Request Management ---");
            Console.WriteLine("1. View my leave requests");
            Console.WriteLine("2. View pending leave requests (for approval)");
            Console.WriteLine("3. Submit new leave request");
            Console.WriteLine("4. Back to Main Menu");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ViewMyLeaveRequests();
                    break;
                case "2":
                    if (loggedInUser.Role.CanApproveLeave())
                    {
                        ViewPendingLeaveRequests();
                    }
                    else
                    {
                        Console.WriteLine("You don't have permission to approve leave requests.");
                        Console.ReadLine();
                    }
                    break;
                case "3":
                    SubmitLeaveRequest();
                    break;
                case "4":
                    currentState = AppState.MainMenu;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }
    private static void ViewMyLeaveRequests()
    {
        Console.Clear();
        Console.WriteLine("\n--- My Leave Requests ---");
        var requests = leaveService.GetByUserId(loggedInUser.Id);

        if (requests.Any())
        {
            Console.WriteLine("ID\tStart Date\tEnd Date\tDuration\tStatus\tDescription");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var req in requests)
            {
                Console.WriteLine($"{req.Id}\t{req.StartDate.ToShortDateString()}\t{req.EndDate.ToShortDateString()}\t{req.Duration} days\t{req.Status}\t{req.Description}");
            }
        }
        else
        {
            Console.WriteLine("No leave requests found.");
        }

        Console.WriteLine($"\nRemaining leave days: {loggedInUser.RemainingLeaveDays}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ViewPendingLeaveRequests()
    {
        Console.Clear();
        Console.WriteLine("\n--- Pending Leave Requests ---");
        var requests = leaveService.GetPendingLeaveRequests();

        if (requests.Any())
        {
            Console.WriteLine("ID\tUser\tStart Date\tEnd Date\tDuration\tDescription");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var req in requests)
            {
                var user = userService.GetUserById(req.UserId);
                Console.WriteLine($"{req.Id}\t{user.Name}\t{req.StartDate.ToShortDateString()}\t{req.EndDate.ToShortDateString()}\t{req.Duration} days\t{req.Description}");
            }

            Console.Write("\nEnter Request ID to approve/reject (or press Enter to go back): ");
            string requestId = Console.ReadLine();

            if (!string.IsNullOrEmpty(requestId))
            {
                ProcessLeaveRequestApproval(requestId);
            }
        }
        else
        {
            Console.WriteLine("No pending leave requests found.");
            Console.ReadLine();
        }
    }

    private static void ProcessLeaveRequestApproval(string requestId)
    {
        Console.WriteLine("\n1. Approve");
        Console.WriteLine("2. Reject");
        Console.Write("Enter your choice: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                leaveService.ApproveRequest(requestId, loggedInUser.Id);
                Console.WriteLine("Leave request approved successfully.");
                break;
            case "2":
                Console.Write("Enter rejection reason: ");
                string reason = Console.ReadLine();
                leaveService.RejectRequest(requestId, loggedInUser.Id, reason);
                Console.WriteLine("Leave request rejected.");
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
        Console.ReadLine();
    }

    private static void SubmitLeaveRequest()
    {
        Console.Clear();
        Console.WriteLine("\n--- Submit Leave Request ---");

        try
        {
            Console.Write("Start Date (yyyy-mm-dd): ");
            DateTime startDate = DateTime.Parse(Console.ReadLine());

            Console.Write("End Date (yyyy-mm-dd): ");
            DateTime endDate = DateTime.Parse(Console.ReadLine());

            Console.Write("Description: ");
            string description = Console.ReadLine();

            var request = leaveService.SubmitRequest(loggedInUser.Id, startDate, endDate, description);
            Console.WriteLine($"Leave request submitted successfully. ID: {request.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting leave request: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowBusinessTripMenu()
    {
        while (currentState == AppState.BusinessTripMenu)
        {
            Console.Clear();
            Console.WriteLine("\n--- Business Trip Management ---");
            Console.WriteLine("1. View my business trip requests");
            Console.WriteLine("2. View pending business trip requests (for approval)");
            Console.WriteLine("3. Submit new business trip request");
            Console.WriteLine("4. Back to Main Menu");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ViewMyBusinessTrips();
                    break;
                case "2":
                    if (loggedInUser.Role.CanApproveTrips())
                    {
                        ViewPendingBusinessTrips();
                    }
                    else
                    {
                        Console.WriteLine("You don't have permission to approve business trip requests.");
                        Console.ReadLine();
                    }
                    break;
                case "3":
                    SubmitBusinessTripRequest();
                    break;
                case "4":
                    currentState = AppState.MainMenu;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    private static void ViewMyBusinessTrips()
    {
        Console.Clear();
        Console.WriteLine("\n--- My Business Trip Requests ---");
        var trips = businessTripService.GetBusinessTripByUserId(loggedInUser.Id);

        if (trips.Any())
        {
            Console.WriteLine("ID\tDestination\tStart Date\tEnd Date\tStatus\tEstimated Cost\tActual Cost");
            Console.WriteLine("------------------------------------------------------------------------------");
            foreach (var trip in trips)
            {
                Console.WriteLine($"{trip.Id}\t{trip.Destination}\t{trip.StartDate.ToShortDateString()}\t{trip.EndDate.ToShortDateString()}\t{trip.Status}\t{trip.EstimatedCost:C}\t{trip.ActualCost:C}");
            }
        }
        else
        {
            Console.WriteLine("No business trip requests found.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ViewPendingBusinessTrips()
    {
        Console.Clear();
        Console.WriteLine("\n--- Pending Business Trip Requests ---");
        var trips = businessTripService.GetPendingBusinessTripRequests();

        if (trips.Any())
        {
            Console.WriteLine("ID\tUser\tDestination\tStart Date\tEnd Date\tEstimated Cost");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var trip in trips)
            {
                var user = userService.GetUserById(trip.UserId);
                Console.WriteLine($"{trip.Id}\t{user.Name}\t{trip.Destination}\t{trip.StartDate.ToShortDateString()}\t{trip.EndDate.ToShortDateString()}\t{trip.EstimatedCost:C}");
            }

            Console.Write("\nEnter Request ID to approve/reject (or press Enter to go back): ");
            string tripId = Console.ReadLine();

            if (!string.IsNullOrEmpty(tripId))
            {
                ProcessBusinessTripApproval(tripId);
            }
        }
        else
        {
            Console.WriteLine("No pending business trip requests found.");
            Console.ReadLine();
        }
    }

    private static void ProcessBusinessTripApproval(string tripId)
    {
        Console.WriteLine("\n1. Approve");
        Console.WriteLine("2. Reject");
        Console.Write("Enter your choice: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                businessTripService.ApproveRequest(tripId, loggedInUser.Id);
                Console.WriteLine("Business trip approved successfully.");
                break;
            case "2":
                Console.Write("Enter rejection reason: ");
                string reason = Console.ReadLine();
                businessTripService.RejectRequest(tripId, loggedInUser.Id, reason);
                Console.WriteLine("Business trip rejected.");
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
        Console.ReadLine();
    }

    private static void SubmitBusinessTripRequest()
    {
        Console.Clear();
        Console.WriteLine("\n--- Submit Business Trip Request ---");

        try
        {
            Console.Write("Destination: ");
            string destination = Console.ReadLine();

            Console.Write("Start Date (yyyy-mm-dd): ");
            DateTime startDate = DateTime.Parse(Console.ReadLine());

            Console.Write("End Date (yyyy-mm-dd): ");
            DateTime endDate = DateTime.Parse(Console.ReadLine());

            Console.Write("Purpose: ");
            string purpose = Console.ReadLine();

            Console.Write("Estimated Cost: ");
            decimal estimatedCost = decimal.Parse(Console.ReadLine());

            var trip = businessTripService.SubmitRequest(
                loggedInUser.Id, destination, startDate, endDate, purpose, estimatedCost);

            Console.WriteLine($"Business trip request submitted successfully. ID: {trip.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting business trip request: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowPayrollReportMenu()
    {
        while (currentState == AppState.PayrollReportMenu)
        {
            Console.Clear();
            Console.WriteLine("\n--- Payroll Report ---");
            Console.WriteLine("1. View payroll by user");
            Console.WriteLine("2. View payroll by period");
            Console.WriteLine("3. Generate payroll for user");
            Console.WriteLine("4. Mark payroll as paid");
            Console.WriteLine("5. Back to Main Menu");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ViewPayrollByUser();
                    break;
                case "2":
                    ViewPayrollByPeriod();
                    break;
                case "3":
                    GeneratePayroll();
                    break;
                case "4":
                    MarkPayrollAsPaid();
                    break;
                case "5":
                    currentState = AppState.MainMenu;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }
    private static void ViewPayrollByUser()
    {
        Console.Clear();
        Console.WriteLine("\n--- Payroll by User ---");

        Console.Write("Enter User ID (leave blank for all users): ");
        string userId = Console.ReadLine();

        IEnumerable<Payroll> payrolls;

        if (string.IsNullOrEmpty(userId))
        {
            payrolls = payrollService.GetPayrollByPeriod(DateTime.Now.AddMonths(-6), DateTime.Now);
        }
        else
        {
            payrolls = payrollService.GetUserPayrolls(userId);
        }

        if (payrolls.Any())
        {
            Console.WriteLine("ID\tUser\tPeriod\tBasic Salary\tAllowances\tTotal\tPayment Date\tStatus");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var payroll in payrolls)
            {
                var user = userService.GetUserById(payroll.UserId);
                Console.WriteLine($"{payroll.Id}\t{user.Name}\t{payroll.PeriodStart.ToShortDateString()} - {payroll.PeriodEnd.ToShortDateString()}\t{payroll.BasicSalary:C}\t{payroll.TravelAllowance + payroll.MealAllowance + payroll.OtherAllowances:C}\t{payroll.TotalSalary:C}\t{payroll.PaymentDate.ToShortDateString()}\t{(payroll.IsPaid ? "Paid" : "Pending")}");
            }
        }
        else
        {
            Console.WriteLine("No payroll records found.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ViewPayrollByPeriod()
    {
        Console.Clear();
        Console.WriteLine("\n--- Payroll by Period ---");

        Console.Write("Start Date (yyyy-mm-dd): ");
        DateTime startDate = DateTime.Parse(Console.ReadLine());

        Console.Write("End Date (yyyy-mm-dd): ");
        DateTime endDate = DateTime.Parse(Console.ReadLine());

        var payrolls = payrollService.GetPayrollByPeriod(startDate, endDate);

        if (payrolls.Any())
        {
            Console.WriteLine("ID\tUser\tPeriod\tBasic Salary\tAllowances\tTotal\tPayment Date\tStatus");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var payroll in payrolls)
            {
                var user = userService.GetUserById(payroll.UserId);
                Console.WriteLine($"{payroll.Id}\t{user.Name}\t{payroll.PeriodStart.ToShortDateString()} - {payroll.PeriodEnd.ToShortDateString()}\t{payroll.BasicSalary:C}\t{payroll.TravelAllowance + payroll.MealAllowance + payroll.OtherAllowances:C}\t{payroll.TotalSalary:C}\t{payroll.PaymentDate.ToShortDateString()}\t{(payroll.IsPaid ? "Paid" : "Pending")}");
            }
        }
        else
        {
            Console.WriteLine("No payroll records found for this period.");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void GeneratePayroll()
    {
        Console.Clear();
        Console.WriteLine("\n--- Generate Payroll ---");

        Console.Write("User ID: ");
        string userId = Console.ReadLine();

        Console.Write("Period Start (yyyy-mm-dd): ");
        DateTime periodStart = DateTime.Parse(Console.ReadLine());

        Console.Write("Period End (yyyy-mm-dd): ");
        DateTime periodEnd = DateTime.Parse(Console.ReadLine());

        try
        {
            var payroll = payrollService.GeneratePayroll(userId, periodStart, periodEnd);
            Console.WriteLine($"Payroll generated successfully. ID: {payroll.Id}");
            Console.WriteLine($"Basic Salary: {payroll.BasicSalary:C}");
            Console.WriteLine($"Travel Allowance: {payroll.TravelAllowance:C}");
            Console.WriteLine($"Meal Allowance: {payroll.MealAllowance:C}");
            Console.WriteLine($"Total Salary: {payroll.TotalSalary:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating payroll: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void MarkPayrollAsPaid()
    {
        Console.Clear();
        Console.WriteLine("\n--- Mark Payroll as Paid ---");

        Console.Write("Payroll ID: ");
        string payrollId = Console.ReadLine();

        try
        {
            payrollService.MarkAsPaid(payrollId);
            Console.WriteLine("Payroll marked as paid successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking payroll as paid: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}

// Mock implementations of the repositories for demonstration purposes
public class MockUserRepository : IUserService
{
    private readonly List<User> _users = new List<User>();

    public User GetUserById(string id) => _users.FirstOrDefault(u => u.Id == id);
    public IEnumerable<User> GetAllUsers() => _users;
    public void AddUser(User user) => _users.Add(user);
    public void UpdateUser(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            _users.Remove(existing);
            _users.Add(user);
        }
    }
    public void DeleteUser(string id) => _users.RemoveAll(u => u.Id == id);
    public bool Exists(string email) => _users.Any(u => u.Email == email);
}

public class MockLeaveRequestRepository : ILeaveRequestRepository
{
    private readonly List<LeaveRequest> _requests = new List<LeaveRequest>();

    public LeaveRequest GetLeaveById(string id) => _requests.FirstOrDefault(r => r.Id == id);
    public IEnumerable<LeaveRequest> GetByUserId(string userId) => _requests.Where(r => r.UserId == userId);
    public IEnumerable<LeaveRequest> GetPendingLeaveRequests() => _requests.Where(r => r.Status == RequestStatus.Pending);
    public void AddPendingRequest(LeaveRequest request) => _requests.Add(request);
    public void UpdatePendingRequest(LeaveRequest request)
    {
        var existing = _requests.FirstOrDefault(r => r.Id == request.Id);
        if (existing != null)
        {
            _requests.Remove(existing);
            _requests.Add(request);
        }
    }
}

public class MockBusinessTripRepository : IBusinessTripRepository
{
    private readonly List<BusinessTrip> _trips = new List<BusinessTrip>();

    public BusinessTrip GetBusinessTripById(string id) => _trips.FirstOrDefault(t => t.Id == id);
    public IEnumerable<BusinessTrip> GetBusinessTripByUserId(string userId) => _trips.Where(t => t.UserId == userId);
    public IEnumerable<BusinessTrip> GetPendingBusinessRequest() => _trips.Where(t => t.Status == RequestStatus.Pending);
    public void Add(BusinessTrip businessTrip) => _trips.Add(businessTrip);
    public void Update(BusinessTrip businessTrip)
    {
        var existing = _trips.FirstOrDefault(t => t.Id == businessTrip.Id);
        if (existing != null)
        {
            _trips.Remove(existing);
            _trips.Add(businessTrip);
        }
    }
}

public class MockPayrollRepository : IPayrollRepository
{
    private readonly List<Payroll> _payrolls = new List<Payroll>();

    public Payroll GetPayrollById(string id) => _payrolls.FirstOrDefault(p => p.Id == id);
    public IEnumerable<Payroll> GetPayrollByUserId(string userId) => _payrolls.Where(p => p.UserId == userId);
    public IEnumerable<Payroll> GetPayrollByPeriod(DateTime start, DateTime end) =>
        _payrolls.Where(p => p.PeriodStart >= start && p.PeriodEnd <= end);
    public void Add(Payroll payroll) => _payrolls.Add(payroll);
    public void Update(Payroll payroll)
    {
        var existing = _payrolls.FirstOrDefault(p => p.Id == payroll.Id);
        if (existing != null)
        {
            _payrolls.Remove(existing);
            _payrolls.Add(payroll);
        }
    }
}
