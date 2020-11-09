namespace TDDMockingAutoFixture.Tests.Payroll
{
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using FluentAssertions;
    using Moq;
    using TDDMockingAutoFixture.DataLayer;
    using TDDMockingAutoFixture.Models;
    using TDDMockingAutoFixture.Payroll;
    using Xunit;

    public class PayrollRunnerTests
    {
        private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

        public PayrollRunnerTests()
        {
            this.fixture.Freeze<Mock<IRepository<Employee>>>();
            this.fixture.Register<IPayrollRunner>(() => this.fixture.Create<PayrollRunner>());
        }

        [Fact]
        public void RunPayroll_WithNoEmployees_Success()
        {
            // Arrange
            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(Enumerable.Empty<Employee>());

            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            var result = sut.RunPayroll();

            // Assert
            result.Errors.Should().BeEmpty();
        }
    }
}
