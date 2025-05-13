using System;
using System.Collections.Generic;

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
    // Simple mock data storage
    private static List<User> users = new List<User>();
    private static List<LeaveRequest> leaveRequests = new List<LeaveRequest>();
    private static List<BusinessTrip> businessTrips = new List<BusinessTrip>();
    // No mock data needed for payroll report in this basic example

    private static User loggedInUser = null;
    private static AppState currentState = AppState.Login;

    public static void Main(string[] args)
    {
        // Initialize some mock users
        users.Add(new User { Username = "admin", Password = "password", Role = "system admin" });
        users.Add(new User { Username = "employee", Password = "password", Role = "employee" });

        Console.WriteLine("Welcome to the Company Management System!");

        // Main application loop (Automata)
        while (currentState != AppState.Exit)
        {
            ProcessState();
        }

        Console.WriteLine("Thank you for using the Company Management System!");
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
        Console.WriteLine("\n--- Login ---");
        Console.Write("Username: ");
        string username = Console.ReadLine();
        Console.Write("Password: ");
        string password = Console.ReadLine();

        loggedInUser = users.FirstOrDefault(u => u.Username == username && u.Password == password);

        if (loggedInUser != null)
        {
            Console.WriteLine("Login successful!");
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
        Console.WriteLine($"\n--- Main Menu ({loggedInUser.Role}) ---");
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
                currentState = AppState.UserManagementMenu;
                break;
            case "2":
                currentState = AppState.LeaveRequestMenu;
                break;
            case "3":
                currentState = AppState.BusinessTripMenu;
                break;
            case "4":
                currentState = AppState.PayrollReportMenu;
                break;
            case "5":
                loggedInUser = null;
                currentState = AppState.Login;
                Console.WriteLine("Logged out successfully.");
                break;
            case "6":
                currentState = AppState.Exit;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowUserManagementMenu()
    {
        Console.Clear();
        Console.WriteLine("\n--- User Management ---");
        Console.WriteLine("1. View all users");
        Console.WriteLine("2. Add new user (Mock)");
        Console.WriteLine("3. Edit user (Mock)");
        Console.WriteLine("4. Delete user (Mock)");
        Console.WriteLine("5. Back to Main Menu");
        Console.Write("Enter your choice: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.WriteLine("\n--- All Users ---");
                if (users.Any())
                {
                    foreach (var user in users)
                    {
                        Console.WriteLine($"- Username: {user.Username}, Role: {user.Role}");
                    }
                }
                else
                {
                    Console.WriteLine("No users found.");
                }
                break;
            case "2":
            case "3":
            case "4":
                Console.WriteLine("This functionality is mocked and not implemented.");
                break;
            case "5":
                currentState = AppState.MainMenu;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowLeaveRequestMenu()
    {
        Console.Clear();
        Console.WriteLine("\n--- Leave Request Management ---");
        Console.WriteLine("1. View all leave requests");
        Console.WriteLine("2. Submit new leave request (Mock)");
        Console.WriteLine("3. Approve/Reject leave request (Mock)");
        Console.WriteLine("4. Back to Main Menu");
        Console.Write("Enter your choice: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.WriteLine("\n--- All Leave Requests ---");
                if (leaveRequests.Any())
                {
                    foreach (var request in leaveRequests)
                    {
                        Console.WriteLine($"- User: {request.Username}, Start Date: {request.StartDate.ToShortDateString()}, End Date: {request.EndDate.ToShortDateString()}, Status: {request.Status}");
                    }
                }
                else
                {
                    Console.WriteLine("No leave requests found.");
                }
                break;
            case "2":
            case "3":
                Console.WriteLine("This functionality is mocked and not implemented.");
                break;
            case "4":
                currentState = AppState.MainMenu;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowBusinessTripMenu()
    {
        Console.Clear();
        Console.WriteLine("\n--- Business Trip Management ---");
        Console.WriteLine("1. View all business trip requests");
        Console.WriteLine("2. Submit new business trip request (Mock)");
        Console.WriteLine("3. Approve/Reject business trip request (Mock)");
        Console.WriteLine("4. Back to Main Menu");
        Console.Write("Enter your choice: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.WriteLine("\n--- All Business Trip Requests ---");
                if (businessTrips.Any())
                {
                    foreach (var trip in businessTrips)
                    {
                        Console.WriteLine($"- User: {trip.Username}, Destination: {trip.Destination}, Start Date: {trip.StartDate.ToShortDateString()}, Status: {trip.Status}");
                    }
                }
                else
                {
                    Console.WriteLine("No business trip requests found.");
                }
                break;
            case "2":
            case "3":
                Console.WriteLine("This functionality is mocked and not implemented.");
                break;
            case "4":
                currentState = AppState.MainMenu;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowPayrollReportMenu()
    {
        Console.Clear();
        Console.WriteLine("\n--- Payroll Report ---");
        Console.WriteLine("This section would display payroll data.");
        Console.WriteLine("Functionality is mocked and not implemented.");
        Console.WriteLine("1. Back to Main Menu");
        Console.Write("Enter your choice: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                currentState = AppState.MainMenu;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    // Simple mock data classes
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // employee, hr, supervisor, financing, system admin
    }

    public class LeaveRequest
    {
        public string Username { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
    }

    public class BusinessTrip
    {
        public string Username { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
    }
}
