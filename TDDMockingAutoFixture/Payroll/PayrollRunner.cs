namespace TDDMockingAutoFixture.Payroll
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using TDDMockingAutoFixture.DataLayer;
    using TDDMockingAutoFixture.Models;

    public class PayrollRunner : IPayrollRunner
    {
        private readonly IRepository<Employee> employeeRepository;

        private readonly IExternalPayrollProvider externalPayrollProvider;

        public PayrollRunner(IRepository<Employee> employeeRepository, IExternalPayrollProvider externalPayrollProvider)
        {
            this.employeeRepository = employeeRepository;
            this.externalPayrollProvider = externalPayrollProvider;
        }

        public PayrollResult RunPayroll()
        {
            var employees = this.employeeRepository.GetAll();
            
            List<dynamic> payrollRun = new List<dynamic>();

            foreach (var employee in employees)
            {
                payrollRun.Add(new
                {
                    EmployeeId = employee.Id,
                    NetPay = employee.GrossPay
                });
            }

            try
            {
                if (payrollRun.Any())
                {
                    var result = this.externalPayrollProvider.RunPayroll(JsonConvert.SerializeObject(payrollRun));
                    var errors = (List<PayrollProviderResult>)JsonConvert.DeserializeObject(
                        result, typeof(List<PayrollProviderResult>));

                    return new PayrollResult
                    {
                        Errors = errors != null && errors.Any() 
                            ? errors.ToDictionary(
                            error => error.EmployeeId, error => $"{error.FailureCode} :{error.FailureReason}")
                            : null
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }

            return new PayrollResult();
        }
    }
}