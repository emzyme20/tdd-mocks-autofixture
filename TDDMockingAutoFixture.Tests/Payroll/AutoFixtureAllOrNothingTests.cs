namespace TDDMockingAutoFixture.Tests.Payroll
{
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Moq;
    using Newtonsoft.Json;
    using TDDMockingAutoFixture.DataLayer;
    using TDDMockingAutoFixture.Models;
    using TDDMockingAutoFixture.Payroll;
    using Xunit;

    public class AutoFixtureAllOrNothingTests
    {
        private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

        public AutoFixtureAllOrNothingTests()
        {
            this.fixture.Freeze<Mock<IExternalPayrollProvider>>();
            this.fixture.Register<IPayrollRunner>(() => this.fixture.Create<PayrollRunner>());
        }

        [Fact(Skip = "Test will fail because mock not from AutoFixture")]
        // [Fact]
        public void RunPayroll_WithRepositoryMockNotFromAutoFixture()
        {
            // Arrange
            var mockNotFromAutoFixture = new Mock<IRepository<Employee>>();
            mockNotFromAutoFixture
                .Setup(x => x.GetAll())
                .Returns(this.fixture.CreateMany<Employee>(10));

            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            sut.RunPayroll();

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void RunPayroll_InjectMockWorkAround()
        {
            // Arrange
            var mockNotFromAutoFixture = new Mock<IRepository<Employee>>();
            mockNotFromAutoFixture
                .Setup(x => x.GetAll())
                .Returns(this.fixture.CreateMany<Employee>(10));

            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            // Act
            var sut = new PayrollRunner(mockNotFromAutoFixture.Object, this.fixture.Create<IExternalPayrollProvider>());
            sut.RunPayroll();

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());
        }
    }
}