using CoreLibrary.InterfaceLib;
using CoreLibrary.ModelLib;
using Serilog;

namespace CoreLibrary.Repository
{
    public class InMemoryPayrollRepository : IPayrollRepository
    {
        private readonly List<Payroll> _payrolls = new();
        private readonly object _lock = new();
        private readonly ILogger _logger;

        public IEnumerable<Payroll> GetAll()
        {
            lock (_lock)
            {
                _logger.Information($"Getting all business trip");
                return _payrolls.ToList();
            }
        }

        public Payroll GetById(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Getting business trip by ID: {id}");
                var payroll = _payrolls.FirstOrDefault(p => p.Id == id);
                if (payroll == null)
                {
                    _logger.Warning($"Business trip with ID {id} not found");
                    throw new KeyNotFoundException("Business trip not found");
                }
                return payroll;
            }
        }

        public IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end)
        {
            lock (_lock)
            {
                Log.Information($"Getting payrolls between {start} and {end}");
                return _payrolls.Where(p => p.PeriodStart >= start && p.PeriodEnd <= end).ToList();
            }
        }

        public void Add(Payroll payroll)
        {
            lock (_lock)
            {
                Log.Information($"Adding payroll for user ID: {payroll.UserId}");
                _payrolls.Add(payroll);
                Log.Information($"Payroll for user ID {payroll.UserId} added successfully");
            }
        }

        public void Update(Payroll payroll)
        {
            lock (_lock)
            {
                Log.Information($"Updating payroll for user ID: {payroll.UserId}");
                var index = _payrolls.FindIndex(p => p.Id == payroll.Id);
                if (index >= 0)
                {
                    _payrolls[index] = payroll;
                    Log.Information($"Payroll for user ID {payroll.UserId} updated successfully");
                }
                else
                {
                    Log.Warning($"Payroll with ID {payroll.Id} not found for update");
                    throw new KeyNotFoundException("Payroll not found");
                }
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information($"Deleting payroll with ID: {id}");
                var count = _payrolls.RemoveAll(p => p.Id == id);
                if (count > 0)
                {
                    _logger.Warning($"Payroll with ID {id} not found for deletion");
                    throw new KeyNotFoundException("Payroll not found");
                }
                else
                {
                    _logger.Information($"Payroll with ID {id} deleted successfully");
                }
            }
        }
    }
}
