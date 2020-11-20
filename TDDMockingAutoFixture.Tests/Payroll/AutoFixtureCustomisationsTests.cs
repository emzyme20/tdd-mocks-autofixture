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
    using Xunit.Abstractions;

    public class AutoFixtureCustomisationsTests
    {
        private readonly ITestOutputHelper output;

        private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

        public AutoFixtureCustomisationsTests(ITestOutputHelper output)
        {
            this.output = output;
            this.fixture.Freeze<Mock<IRepository<Employee>>>();
            this.fixture.Freeze<Mock<IExternalPayrollProvider>>();
            this.fixture.Register<IPayrollRunner>(() => this.fixture.Create<PayrollRunner>());
        }

        [Fact]
        public void AllEmployeesShouldHavePensionDeduction()
        {
            // Arrange
            var expectedDeduction = new Deduction
            {
                Description = "Pension",
                Percentage = 9
            };

            var employees = this.fixture.CreateMany<Employee>(10);

            if (employees.Any(e => !e.Deductions.Contains(expectedDeduction)))
            {
                this.output.WriteLine("All employees should have a company pension deduction");
            }

            this.fixture.Customize<Employee>(
                composer => composer.With(
                    entity => entity.Deductions, new List<Deduction>
                    {
                        expectedDeduction
                    }));

            var customisedEmployees = this.fixture.CreateMany<Employee>(10);

            if (customisedEmployees.Any(e => !e.Deductions.Contains(expectedDeduction)))
            {
                this.output.WriteLine("All customised employees should have a company pension deduction");
            }
            
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(employees);
            
            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            var result = sut.RunPayroll();

            // Assert
            result.Errors.Should().BeNull();

            foreach (var customisedEmployee in customisedEmployees)
            {
                customisedEmployee.Deductions.Should()
                    .ContainEquivalentOf(expectedDeduction, "Employee should have pension deduction");
            }
        }
    }
}