using System;
using System.Collections.Generic;
using CoreLibrary;
using User = CoreLibrary.ModelLib.User;
using LeaveRequest = CoreLibrary.ModelLib.LeaveRequest;
using BusinessTrip = CoreLibrary.ModelLib.BusinessTrip;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Serilog;
using CoreLibrary.ModelLib;
using CoreLibrary.InterfaceLib;
using CoreLibrary.Repository;
using CoreLibrary.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

Log.Logger = CoreLibrary.LoggerConfig.ConfigureLogger();

// CLI + API

try
{
    var builder = WebApplication.CreateBuilder(args);

    // API-specific services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
    builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    builder.Services.AddSingleton<ILeaveRequestRepository, InMemoryLeaveRequestRepository>();
    builder.Services.AddSingleton<IBusinessTripRepository, InMemoryBusinessTripRepository>();
    builder.Services.AddSingleton<IPayrollRepository, InMemoryPayrollRepository>();

    builder.Services.AddSingleton<UserService>();
    builder.Services.AddSingleton<LeaveService>();
    builder.Services.AddSingleton<BusinessTripService>();
    builder.Services.AddSingleton<PayrollService>();

    builder.Services.AddSingleton<IUserService, UserService>();
    builder.Services.AddSingleton<ILeaveRequestService, LeaveService>();
    builder.Services.AddSingleton<IBusinessTripService, BusinessTripService>();
    builder.Services.AddSingleton<IPayrollService, PayrollService>();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    var serviceProvider = builder.Services.BuildServiceProvider();

    var userService = serviceProvider.GetRequiredService<UserService>();
    var leaveService = serviceProvider.GetRequiredService<LeaveService>();
    var businessTripService = serviceProvider.GetRequiredService<BusinessTripService>();
    var payrollService = serviceProvider.GetRequiredService<PayrollService>();

    InitializeTestData(userService);

    var apiTask = Task.Run(() => app.RunAsync());

    var cli = new Main(userService, leaveService, businessTripService, payrollService);

    await cli.MainCLI();

    await app.StopAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

void InitializeTestData(UserService userService)
{
    try
    {
        userService.Register("Fizryan", "fizryan@mail.com", "password123", Role.SysAdmin, 50000000);
        userService.Register("Naufal", "naufal@mail.com", "pass0000122231", Role.Employee, 2000000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing test data: {ex.Message}");
    }
}

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

public class Main
{
    private static UserService userService;
    private static LeaveService leaveService;
    private static BusinessTripService businessTripService;
    private static PayrollService payrollService;

    private static User loggedInUser = null;
    private static AppState currentState = AppState.Login;
    private static AppState previousState = AppState.Login;

    public Main(UserService userSvc, LeaveService leaveSvc, BusinessTripService businessTripSvc, PayrollService payrollSvc)
    {
        userService = userSvc;
        leaveService = leaveSvc;
        businessTripService = businessTripSvc;
        payrollService = payrollSvc;
    }

    public async Task MainCLI()
    {
        Console.WriteLine("Welcome to the Company Management System!");

        // Main application loop (Automata)
        while (currentState != AppState.Exit)
        {
            try
            {
                ProcessState();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
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

    private static void HandleError(Exception ex)
    {
        Console.Clear();
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
        currentState = previousState;
    }

    private static void TransitionTo(AppState newState)
    {
        previousState = currentState;
        currentState = newState;
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
            TransitionTo(AppState.MainMenu);
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
        var choice = Helper.ShowMenu(
            $"Main Menu ({loggedInUser.Name} - {loggedInUser.Role})",
            new Dictionary<string, string>
            {
                ["1"] = "User Management",
                ["2"] = "Leave Request",
                ["3"] = "Business Trip Management",
                ["4"] = "Payroll Report",
                ["5"] = "Logout",
                ["6"] = "Exit"
            });

        Helper.HandleMenuSelection(choice, new Dictionary<string, Action>
        {
            ["1"] = () =>
            {
                if (loggedInUser.Role.CanManageUsers())
                    TransitionTo(AppState.UserManagementMenu);
                else
                {
                    Console.WriteLine("\nYou don't have permission to access this menu.");
                    Console.ReadLine();
                }
            },
            ["2"] = () => TransitionTo(AppState.LeaveRequestMenu),
            ["3"] = () => TransitionTo(AppState.BusinessTripMenu),
            ["4"] = () =>
            {
                if (loggedInUser.Role.CanManagePayroll())
                    TransitionTo(AppState.PayrollReportMenu);
                else
                {
                    Console.WriteLine("\nYou don't have permission to access this menu.");
                    Console.ReadLine();
                }
            },
            ["5"] = () =>
            {
                loggedInUser = null;
                TransitionTo(AppState.Login);
                Console.WriteLine("Logged out successfully.");
            },
            ["6"] = () => TransitionTo(AppState.Exit)
        });
    }

    private static void ShowUserManagementMenu()
    {
        var choice = Helper.ShowMenu(
            "User Management",
            new Dictionary<string, string>
            {
                ["1"] = "View all users",
                ["2"] = "Add new user",
                ["3"] = "Edit user",
                ["4"] = "Delete user",
                ["5"] = "Back to Main Menu"
            });

        Helper.HandleMenuSelection(choice, new Dictionary<string, Action>
        {
            ["1"] = () => ViewAllUsers(),
            ["2"] = () => AddNewUser(),
            ["3"] = () => EditUser(),
            ["4"] = () => DeleteUser(),
            ["5"] = () => TransitionTo(AppState.MainMenu),
        });
    }

    private static void ViewAllUsers()
    {
        Console.Clear();
        Console.WriteLine("\n--- All Users ---");
        var users = userService.GetAllUser();

        if (users.Any())
        {
            Console.WriteLine("ID\tName\tEmail\tRole\tJoin Date\tStatus");
            Console.WriteLine("--------------------------------------------------");
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id}\t{user.Name}\t{user.Email}\t\t{user.Role}\t{user.JoinDate.ToShortDateString()}");
            }
        }
        else
        {
            Console.WriteLine("No users found.");
        }
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();

        Console.WriteLine("WIP");
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

        Console.WriteLine("Salary: ");
        string salary = Console.ReadLine();
        if (salary == null) { salary = "0"; }

        Console.WriteLine("Available Roles:");
        foreach (var role in Enum.GetValues(typeof(Role)))
        {
            Console.WriteLine($"{(int)role}. {role}");
        }
        Console.Write("Select Role (number): ");
        Role roleSelected = (Role)Enum.Parse(typeof(Role), Console.ReadLine());

        try
        {
            var newUser = userService.Register(name, email, password, roleSelected, int.Parse(salary));
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
        int userId = int.Parse(Console.ReadLine());

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
        int userId = int.Parse(Console.ReadLine());

        try 
        {
            var user = userService.GetUserById(userId);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            Console.WriteLine("1. Delete user permanently");
            Console.WriteLine("2. Cancel");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Are you sure you want to permanently delete this user? (Y/N): ");
                    if (Console.ReadLine().Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        userService.DeleteUserAccount(loggedInUser.Id, userId);
                        Console.WriteLine("User deleted.");
                    }
                    break;
                case "2":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deactivating user: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowLeaveRequestMenu()
    {
        var choice = Helper.ShowMenu(
            "Leave Request Management",
            new Dictionary<string, string>
            {
                ["1"] = "View my leave requests",
                ["2"] = "View pending leave requests (for approval)",
                ["3"] = "Submit new leave request",
                ["4"] = "Back to Main Menu"
            });

        Helper.HandleMenuSelection(choice, new Dictionary<string, Action>
        {
            ["1"] = () => ViewMyLeaveRequests(),
            ["2"] = () => {
                if (loggedInUser.Role.CanApproveLeave())
                {
                    ViewPendingLeaveRequests();
                }
                else
                {
                    Console.WriteLine("\nYou don't have permission to approve leave requests.");
                    Console.ReadLine();
                }
            },
            ["3"] = () => SubmitLeaveRequest(),
            ["4"] = () => TransitionTo(AppState.MainMenu)
        });
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
        var requests = leaveService.GetPendingRequest();

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
            string requestIdStr = Console.ReadLine();

            if (!string.IsNullOrEmpty(requestIdStr))
            {
                int requestId = int.Parse(requestIdStr);
                ProcessLeaveRequestApproval(requestId);
            }
        }
        else
        {
            Console.WriteLine("No pending leave requests found.");
            Console.ReadLine();
        }
    }

    private static void ProcessLeaveRequestApproval(int requestId)
    {
        Console.WriteLine("\n1. Approve");
        Console.WriteLine("2. Reject");
        Console.Write("Enter your choice: ");
        string choice = Console.ReadLine();

        try
        {
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
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
        var choice = Helper.ShowMenu(
            "Business Trip Management",
            new Dictionary<string, string>
            {
                ["1"] = "View my business trip requests",
                ["2"] = "View pending business trip requests (for approval)",
                ["3"] = "Submit new business trip request",
                ["4"] = "Update actual trip cost",
                ["5"] = "Back to Main Menu"
            });

        Helper.HandleMenuSelection(choice, new Dictionary<string, Action>
        {
            ["1"] = () => ViewMyBusinessTrips(),
            ["2"] = () => {
                if (loggedInUser.Role.CanApproveTrips())
                {
                    ViewPendingBusinessTrips();
                }
                else
                {
                    Console.WriteLine("\nYou don't have permission to approve business trip requests.");
                    Console.ReadLine();
                }
            },
            ["3"] = () => SubmitBusinessTripRequest(),
            ["4"] = () => UpdateBusinessTripCost(),
            ["5"] = () => TransitionTo(AppState.MainMenu),
        });
    }

    private static void ViewMyBusinessTrips()
    {
        Console.Clear();
        Console.WriteLine("\n--- My Business Trip Requests ---");
        var trips = businessTripService.GetByUserId(loggedInUser.Id);

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
        var trips = businessTripService.GetPendingRequests();

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
            string tripIdStr = Console.ReadLine();

            if (!string.IsNullOrEmpty(tripIdStr))
            {
                int tripId = int.Parse(tripIdStr);
                ProcessBusinessTripApproval(tripId);
            }
        }
        else
        {
            Console.WriteLine("No pending business trip requests found.");
            Console.ReadLine();
        }
    }

    private static void ProcessBusinessTripApproval(int tripId)
    {
        Console.WriteLine("\n1. Approve");
        Console.WriteLine("2. Reject");
        Console.WriteLine("3. Cancel");
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
            case "3":
                return;
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

    private static void UpdateBusinessTripCost()
    {
        Console.Clear();
        Console.WriteLine("\n--- Update Business Trip Cost ---");

        try
        {
            Console.Write("Enter Trip ID: ");
            string tripIdStr = Console.ReadLine();
            if ( !string.IsNullOrEmpty( tripIdStr ) )
            {
                int tripId = int.Parse( tripIdStr );

                Console.Write("Enter Actual Cost: ");
                decimal actualCost = decimal.Parse(Console.ReadLine());

                businessTripService.UpdateActualCost(tripId, actualCost);
                Console.WriteLine("Business trip cost updated successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating business trip cost: {ex.Message}");
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    private static void ShowPayrollReportMenu()
    {
        var choice = Helper.ShowMenu(
            "Payroll Report",
            new Dictionary<string, string>
            {
                ["1"] = "View payroll by user",
                ["2"] = "View payroll by period",
                ["3"] = "Generate payroll for user",
                ["4"] = "Mark payroll as paid",
                ["5"] = "Back to Main Menu"
            });

        Helper.HandleMenuSelection(choice, new Dictionary<string, Action>
        {
            ["1"] = () => ViewPayrollByUser(),
            ["2"] = () => ViewPayrollByPeriod(),
            ["3"] = () => GeneratePayroll(),
            ["4"] = () => MarkPayrollAsPaid(),
            ["5"] = () => TransitionTo(AppState.MainMenu),
        });
    }
    private static void ViewPayrollByUser()
    {
        Console.Clear();
        Console.WriteLine("\n--- Payroll by User ---");

        Console.Write("Enter User ID (leave blank for all users): ");
        string userIdStr = Console.ReadLine();

        IEnumerable<Payroll> payrolls;

        if (string.IsNullOrEmpty(userIdStr))
        {
            payrolls = payrollService.GetPayrollsByPeriod(DateTime.Now.AddMonths(-6), DateTime.Now);
        }
        else
        {
            int userId = int.Parse(userIdStr);
            payrolls = payrollService.GetPayrollsByUserId(userId);
        }

        if (payrolls.Any())
        {
            Console.WriteLine("ID\tUser\tPeriod\tBasic Salary\tPayment Date\tStatus");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var payroll in payrolls)
            {
                var user = userService.GetUserById(payroll.UserId);
                Console.WriteLine($"{payroll.Id}\t{user.Name}\t{payroll.PeriodStart.ToShortDateString()} - {payroll.PeriodEnd.ToShortDateString()}\t{payroll.BasicSalary:C}\t{payroll.PaymentDate.ToShortDateString()}\t{(payroll.IsPaid ? "Paid" : "Pending")}");
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

        var payrolls = payrollService.GetPayrollsByPeriod(startDate, endDate);

        if (payrolls.Any())
        {
            Console.WriteLine("ID\tUser\tPeriod\tBasic Salary\tPayment Date\tStatus");
            Console.WriteLine("------------------------------------------------------------------");
            foreach (var payroll in payrolls)
            {
                var user = userService.GetUserById(payroll.UserId);
                Console.WriteLine($"{payroll.Id}\t{user.Name}\t{payroll.PeriodStart.ToShortDateString()} - {payroll.PeriodEnd.ToShortDateString()}\t{payroll.BasicSalary:C}\t{payroll.PaymentDate.ToShortDateString()}\t{(payroll.IsPaid ? "Paid" : "Pending")}");
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
        string userIdStr = Console.ReadLine();

        Console.Write("Period Start (yyyy-mm-dd): ");
        DateTime periodStart = DateTime.Parse(Console.ReadLine());

        Console.Write("Period End (yyyy-mm-dd): ");
        DateTime periodEnd = DateTime.Parse(Console.ReadLine());

        try
        {
            int userId = int.Parse(userIdStr);
            var payroll = payrollService.GeneratePayroll(userId, periodStart, periodEnd);
            Console.WriteLine($"Payroll generated successfully. ID: {payroll.Id}");
            Console.WriteLine($"Basic Salary: {payroll.BasicSalary:C}");
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
        string payrollIdStr = Console.ReadLine();

        try
        {
            int payrollId = int.Parse(payrollIdStr);
            payrollService.MarkAsPaid(payrollId, loggedInUser.Id);
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

public static class Helper
{
    public static string ShowMenu(string title, Dictionary<string, string> options)
    {
        Console.Clear();
        Console.WriteLine($"\n--- {title} ---");
        foreach (var option in options)
        {
            Console.WriteLine($"{option.Key}. {option.Value}");
        }
        Console.Write("Enter your choice: ");
        return Console.ReadLine();
    }

    public static void HandleMenuSelection(
        string choice,
        Dictionary<string, Action> handlers)
    {
        if (handlers.TryGetValue(choice, out var handle))
        {
            handle();
        }
        else
        {
            Console.WriteLine("Invalid choice. Please try again.");
            Console.ReadLine();
        }
    }
}