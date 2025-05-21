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

        public InMemoryPayrollRepository(ILogger logger)
        {
            _logger = logger.ForContext<InMemoryPayrollRepository>();
        }

        public IEnumerable<Payroll> GetAll()
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving all payrolls");
                return new List<Payroll>(_payrolls);
            }
        }

        public Payroll GetById(int id)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving payroll by ID: {PayrollId}", id);
                return FindPayroll(p => p.Id == id) ?? throw PayrollNotFound(id);
            }
        }

        public IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end)
        {
            lock (_lock)
            {
                _logger.Debug("Retrieving payrolls between {StartDate} and {EndDate}",
                    start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
                return _payrolls.Where(p =>
                    p.PeriodStart >= start && p.PeriodEnd <= end).ToList();
            }
        }

        public void Add(Payroll payroll)
        {
            lock (_lock)
            {
                _logger.Information("Adding payroll for user ID: {UserId}", payroll.UserId);
                payroll.Id = GenerateId();
                _payrolls.Add(payroll);
                _logger.Information("Payroll added successfully with ID: {PayrollId}", payroll.Id);
            }
        }

        public void Update(Payroll payroll)
        {
            lock (_lock)
            {
                _logger.Information("Updating payroll ID: {PayrollId}", payroll.Id);

                var index = _payrolls.FindIndex(p => p.Id == payroll.Id);
                if (index < 0)
                {
                    _logger.Warning("Payroll ID: {PayrollId} not found for update", payroll.Id);
                    throw PayrollNotFound(payroll.Id);
                }

                _payrolls[index] = payroll;
                _logger.Information("Payroll ID: {PayrollId} updated successfully", payroll.Id);
            }
        }

        public void Delete(int id)
        {
            lock (_lock)
            {
                _logger.Information("Deleting payroll ID: {PayrollId}", id);

                var count = _payrolls.RemoveAll(p => p.Id == id);
                if (count == 0)
                {
                    _logger.Warning("Payroll ID: {PayrollId} not found for deletion", id);
                    throw PayrollNotFound(id);
                }

                _logger.Information("Payroll ID: {PayrollId} deleted successfully", id);
            }
        }

        public int GenerateId()
        {
            lock (_lock)
            {
                _logger.Debug("Generating new payroll ID");
                return _payrolls.Count == 0 ? 1 : _payrolls.Max(p => p.Id) + 1;
            }
        }

        #region Private Helper Methods

        private Payroll? FindPayroll(Func<Payroll, bool> predicate)
        {
            return _payrolls.FirstOrDefault(predicate);
        }

        private KeyNotFoundException PayrollNotFound(int id)
        {
            return new KeyNotFoundException($"Payroll with ID {id} not found");
        }

        #endregion
    }
}