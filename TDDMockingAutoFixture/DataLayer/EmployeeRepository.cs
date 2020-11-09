namespace TDDMockingAutoFixture.DataLayer
{
    using System.Collections.Generic;
    using TDDMockingAutoFixture.Models;

    public class EmployeeRepository : IRepository<Employee>
    {
        public IEnumerable<Employee> GetAll()
        {
            throw new System.NotImplementedException();
        }
    }
}