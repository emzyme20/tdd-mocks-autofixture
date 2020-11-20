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

    public class AvoidMockObjectMethodTests
    {
        private readonly IFixture fixture = new Fixture().Customize(new AutoMoqCustomization());

        private readonly Mock<IExternalPayrollProvider> mockPayrollProvider = new Mock<IExternalPayrollProvider>();

        private readonly Mock<IRepository<Employee>> mockRepository = new Mock<IRepository<Employee>>();

        public AvoidMockObjectMethodTests()
        {
            this.fixture.Freeze<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(this.fixture.CreateMany<Employee>(5));

            this.fixture.Freeze<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            this.fixture.Register<IPayrollRunner>(() => this.fixture.Create<PayrollRunner>());
        }

        [Fact]
        public void UsingObjectIsNecessaryWithoutAutoFixture()
        {
            // Arrange
            this.mockPayrollProvider
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            this.mockRepository
                .Setup(x => x.GetAll())
                .Returns(
                    new List<Employee>
                    {
                        new Employee
                        {
                            FirstName = "Poppy",
                            LastName = "Cat",
                            GrossPay = 2000m,
                            Deductions = new List<Deduction>
                            {
                                new Deduction
                                {
                                    Description = "Pension",
                                    Percentage = 5
                                }
                            }
                        }
                    });

            // Act
            var sut = new PayrollRunner(this.mockRepository.Object, this.mockPayrollProvider.Object);
            var result = sut.RunPayroll();

            // Assert
            this.mockPayrollProvider.Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());

            result.Errors.Should().BeNull();
        }

        [Fact]
        public void UsingObjectIsUnnecessaryWithAutoFixture()
        {
            // Act
            var result = this.fixture.Create<IPayrollRunner>().RunPayroll();

            // Assert
            this.fixture.Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());

            result.Errors.Should().BeNull();
        }

        [Fact]
        public void UsingObjectIsUnnecessaryWithAutoFixture_EvenWhenCreatingInstanceYourself()
        {
            // Arrange
            // We have mocks registered in AutoFixture so it is able to create the instance using the mocks, there is no need to 
            // retrieve the mock and call .Object yourself.
            var sut = new PayrollRunner(
                this.fixture.Create<IRepository<Employee>>(),
                this.fixture.Create<IExternalPayrollProvider>());

            // Act
            var result = sut.RunPayroll();

            // Assert
            this.fixture.Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());

            result.Errors.Should().BeNull();
        }
    }
}