namespace TDDMockingAutoFixture.Tests.Payroll
{
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using TDDMockingAutoFixture.DataLayer;
    using TDDMockingAutoFixture.Models;
    using TDDMockingAutoFixture.Payroll;
    using Xunit;
    using Xunit.Abstractions;

    public class AutoFixtureToFreezeOrNotToFreezeTests
    {
        private readonly ITestOutputHelper output;

        private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

        public AutoFixtureToFreezeOrNotToFreezeTests(ITestOutputHelper output)
        {
            this.output = output;
            this.fixture.Register<IPayrollRunner>(() => this.fixture.Create<PayrollRunner>());
        }

        [Fact(Skip = "Test will fail because we have not frozen the so a new instance is created everytime.")]
        // [Fact]
        public void RunPayroll_WithoutFreezing_WillUseWrongDependencies()
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
            sut.RunPayroll();

            var hashcodeOfExternalProviderA = this.fixture.Create<IExternalPayrollProvider>().GetHashCode();
            this.output.WriteLine($"Hashcode of instance A: {hashcodeOfExternalProviderA}");

            var hashcodeOfExternalProviderB = this.fixture.Create<IExternalPayrollProvider>().GetHashCode();
            this.output.WriteLine($"Hashcode of instance B: {hashcodeOfExternalProviderB}");
            
            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void RunPayroll_WithFreezing_WillUseCorrectDependencies()
        {
            // Arrange
            this.fixture.Freeze<Mock<IRepository<Employee>>>();
            this.fixture.Freeze<Mock<IExternalPayrollProvider>>();

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
            sut.RunPayroll();

            var hashcodeOfExternalProviderA = this.fixture.Create<IExternalPayrollProvider>().GetHashCode();
            this.output.WriteLine($"Hashcode of instance A: {hashcodeOfExternalProviderA}");

            var hashcodeOfExternalProviderB = this.fixture.Create<IExternalPayrollProvider>().GetHashCode();
            this.output.WriteLine($"Hashcode of instance B: {hashcodeOfExternalProviderB}");

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());

            var hashcode = this.fixture.Create<IExternalPayrollProvider>().GetHashCode();
            hashcode.Should().Be(this.fixture.Create<IExternalPayrollProvider>().GetHashCode());
        }
    }
}