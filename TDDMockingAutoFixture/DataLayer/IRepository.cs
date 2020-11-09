namespace TDDMockingAutoFixture.DataLayer
{
    using System.Collections.Generic;

    public interface IRepository<TType>
    {
        IEnumerable<TType> GetAll();
    }
}