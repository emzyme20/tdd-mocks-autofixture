namespace TDDMockingAutoFixture.Tests.DataLayer
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using FluentAssertions;
    using TDDMockingAutoFixture.DataLayer;
    using TDDMockingAutoFixture.Models;
    using Xunit;

    public class EmployeeRepositoryTests
    {
        private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

        public EmployeeRepositoryTests()
        {
            this.fixture.Register<IRepository<Employee>>(() => this.fixture.Create<EmployeeRepository>());
        }

        [Fact]
        public void GetAll_WillReturnAllEmployees()
        {
            // Arrange
            var sut = this.fixture.Create<IRepository<Employee>>();

            // Act, Assert
            sut.GetAll().Should().HaveCountGreaterOrEqualTo(1);
        }
    }
}