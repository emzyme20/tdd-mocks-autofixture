namespace TDDMockingAutoFixture.Payroll
{
    public interface IExternalPayrollProvider
    {
         string RunPayroll(string payload);
    }
}