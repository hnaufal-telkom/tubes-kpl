using CoreLibrary.ModelLib;

namespace CoreLibrary.InterfaceLib
{
    public interface IPayrollRepository
    {
        IEnumerable<Payroll> GetAll();
        Payroll GetById(int id);
        IEnumerable<Payroll> GetByPeriod(DateTime start, DateTime end);
        void Add(Payroll payroll);
        void Update(Payroll payroll);
        void Delete(int id);
        int GenerateId();
    }

    public interface IPayrollService
    {
        Payroll GeneratePayroll(int userId, DateTime periodStart, DateTime periodEnd);
        void MarkAsPaid(int payrollId, int approverId);
        IEnumerable<Payroll> GetPayrollsByUserId(int userId);
        IEnumerable<Payroll> GetPayrollsByPeriod(DateTime start, DateTime end);
    }
}
