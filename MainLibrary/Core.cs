using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainLibrary
{
    public enum RequestStatus { Pending, Approved, Rejected }
    public enum Role { Employee, HRD, Supervisor, Finance, SysAdmin }

    public class User
    {
        public required string ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required Role Role { get; set; }
    }

    public class LeaveRequest
    {
        public required string ID { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public required string Description { get; set; }
        public RequestStatus Status { get; set; }
    }

    public class BusinessTrip
    {
        public required string ID { get; set; }
        public required string Destination { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public required string Description { get; set; }
        public RequestStatus Status { get; set; }
        public required decimal CostEstimation { get; set; }
    }

    public class Salary
    {
        public required string ID { get; set; }
        public required decimal BaseSalary { get; set; }
        public required decimal TravelAllowance { get; set; }
        public required decimal LeaveDeduction { get; set; }
        public DateTime Period { get; set; }
    }

    public class StateMachine<TState, TTrigger>
    {
        private readonly Dictionary<(TState, TTrigger), TState> _transitions = new();
        public TState CurrentState { get; private set; }

        public StateMachine(TState initialState) => CurrentState = initialState;

        public void Configure(TState state, TTrigger trigger, TState nextState) =>
            _transitions[(state, trigger)] = nextState;

        public bool Trigger(TTrigger trigger)
        {
            if (_transitions.TryGetValue((CurrentState, trigger), out var next))
            {
                CurrentState = next;
                return true;
            }
            return false;
        }
    }

    public class TableDrivenValidator
    {
        private readonly Dictionary<string, Regex> _rules = new();

        public TableDrivenValidator(Dictionary<string, string> rules)
        {
            foreach (var rule in rules)
                _rules[rule.Key] = new Regex(rule.Value);
        }

        public bool Validate(string field, string value) =>
            _rules.TryGetValue(field, out var regex) && regex.IsMatch(value);
    }

    public class AppConfig
    {
        public int MaxLoginAttempts { get; set; }
        public required string DefaultUserRole { get; set; }
        public int MaxAnnualLeave { get; set; }
        public bool AllowCollectiveLeave { get; set; }
        public static AppConfig LoadFromFile(string path) => JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path));
    }

    public class ApiClient
    {
        private readonly HttpClient _client;
        public ApiClient(string baseUrl) => _client = new HttpClient { BaseAddress = new Uri(baseUrl) };

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var res = await _client.GetAsync(endpoint);
            res.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
            return await _client.PostAsync(endpoint, content);
        }
    }

    public interface IUserService { void Add(User u); User Find(string id); }
    public interface ILeaveService { void Submit(LeaveRequest c); }
    public interface ITripService { void Submit(BusinessTrip d); }
    public interface IPayrollService { Salary CalculateSlip(string id); }

    public class UserService : IUserService
    {
        private readonly Dictionary<string, User> _data = new();
        public void Add(User u) => _data[u.ID] = u;
        public User Find(string id) => _data.ContainsKey(id) ? _data[id] : null;
    }

    public class LeaveService : ILeaveService
    {
        private readonly List<LeaveRequest> _list = new();
        public void Submit(LeaveRequest c) { c.Status = RequestStatus.Pending; _list.Add(c); }
    }

    public class TripService : ITripService
    {
        private readonly List<BusinessTrip> _list = new();
        public void Submit(BusinessTrip d) { d.Status = RequestStatus.Pending; _list.Add(d); }
    }

    public class PayrollService : IPayrollService
    {
        public Salary CalculateSlip(string id)
        {
            return new Salary
            {
                ID = id,
                BaseSalary = 5000000,
                TravelAllowance = 1000000,
                LeaveDeduction = 500000,
                Period = DateTime.Now
            };
        }
    }


    public static class DateHelper
    {
        public static int CalculateDays(DateTime start, DateTime end) => (int)(end - start).TotalDays + 1;
    }

    public static class Localization
    {
        private static readonly Dictionary<string, string> Dictionary = new()
        {
            {"id_SUBMIT_SUCCESS", "Pengajuan berhasil."},
            {"en_SUBMIT_SUCCESS", "Submission successful."}
        };

        public static string Translate(string key, string lang = "id")
        {
            string dictKey = lang + "_" + key;
            return Dictionary.TryGetValue(dictKey, out var val) ? val : key;
        }
    }

    public static class TestDataFactory
    {
        public static LeaveRequest DummyLeave() => new() { ID = "123", DateStart = DateTime.Today, DateEnd = DateTime.Today.AddDays(2), Description = "Going Home", Status = RequestStatus.Pending };
        public static BusinessTrip DummyTrip() => new() { ID = "123", Destination = "Bandung", DateStart = DateTime.Today, DateEnd = DateTime.Today.AddDays(1), Description = "Meeting", CostEstimation = 1000000, Status = RequestStatus.Pending };
    }
}
