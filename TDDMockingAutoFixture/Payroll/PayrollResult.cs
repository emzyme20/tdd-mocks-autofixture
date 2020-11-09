namespace TDDMockingAutoFixture.Payroll
{
    using System.Collections.Generic;

    public class PayrollResult
    {
        public IDictionary<int, IEnumerable<string>> Errors { get; set; }
    }
}