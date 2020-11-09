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
                    NetPay = CalculateNetPay(employee)
                });
            }

            try
            {
                if (payrollRun.Any())
                {
                    var payload = JsonConvert.SerializeObject(payrollRun);
                    var result = this.externalPayrollProvider.RunPayroll(payload);
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

        private decimal CalculateNetPay(Employee employee)
        {
            var netPay = employee.GrossPay;

            foreach (var deduction in employee.Deductions)
            {
                var deductionValue = employee.GrossPay * ((decimal)deduction.Percentage / 100);
                netPay -= deductionValue;
            }

            return netPay;
        }
    }
}