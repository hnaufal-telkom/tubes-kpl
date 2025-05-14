using MainLibrary;
using System;
using System.Collections.Generic;

namespace WebAPI.Service
{
    public class PayrollService
    {
        private readonly IPayrollRepository _payrollRepository;

        public PayrollService(IPayrollRepository payrollRepository)
        {
            _payrollRepository = payrollRepository;
        }

        public IEnumerable<Payroll> GetPayrollsByPeriod(DateTime start, DateTime end) =>
            _payrollRepository.GetPayrollByPeriod(start, end);

        public Payroll GetPayrollById(string id) =>
            _payrollRepository.GetPayrollById(id);

        public IEnumerable<Payroll> GetPayrollsByUserId(string userId) =>
            _payrollRepository.GetPayrollByUserId(userId);
    }
}
