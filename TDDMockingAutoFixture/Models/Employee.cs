namespace TDDMockingAutoFixture.Models
{
    using System.Collections.Generic;

    public class Employee
    {
        public List<Deduction> Deductions { get; set; }

        public string FirstName { get; set; }

        public decimal GrossPay { get; set; }

        public int Id { get; set; }

        public string LastName { get; set; }
    }
}