namespace TDDMockingAutoFixture.DataLayer
{
    using System.Collections.Generic;
    using TDDMockingAutoFixture.Models;

    public class EmployeeRepository : IRepository<Employee>
    {
        public IEnumerable<Employee> GetAll()
        {
            return new List<Employee>()
            {
                new Employee
                {
                    Id = 1,
                    FirstName = "Ella",
                    LastName = "The-Dog",
                    GrossPay = 2000m
                }
            };
        }
    }
}