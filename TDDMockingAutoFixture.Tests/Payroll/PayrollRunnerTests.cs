namespace TDDMockingAutoFixture.Tests.Payroll
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
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
            this.fixture.Freeze<Mock<IExternalPayrollProvider>>();
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
            result.Errors.Should().BeNull();

            this.fixture.Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public void RunPayroll_WithEmployees_WillCallExternalPayrollProvider()
        {
            // Arrange
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(this.fixture.CreateMany<Employee>(10));
            
            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            var result = sut.RunPayroll();

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());

            result.Errors.Should().BeNull();
        }
    }
}
