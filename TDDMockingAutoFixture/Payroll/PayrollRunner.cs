namespace TDDMockingAutoFixture.Payroll
{
    using TDDMockingAutoFixture.DataLayer;
    using TDDMockingAutoFixture.Models;

    public class PayrollRunner : IPayrollRunner
    {
        private readonly IRepository<Employee> employeeRepository;

        public PayrollRunner(IRepository<Employee> employeeRepository)
        {
            this.employeeRepository = employeeRepository;
        }

        public PayrollResult RunPayroll()
        {
            throw new System.NotImplementedException();
        }
    }
}