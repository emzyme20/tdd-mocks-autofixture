namespace TDDMockingAutoFixture.Tests.Payroll
{
    using System;
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

        [Fact]
        public void RunPayroll_MultipleTimes_WillResultInDifferentResponsesFromExternalPayrollProvider()
        {
            // Arrange
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .SetupSequence(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()))
                .Throws(new Exception("Something went wrong"));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(this.fixture.CreateMany<Employee>(10));
            
            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            sut.RunPayroll();
            sut.RunPayroll();
            Action act3 = () => sut.RunPayroll();

            // Assert
            act3.Should().Throw<Exception>();
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public void RunPayroll_ExternalPayrollProviderUnknownError_WillThrowException()
        {
            // Arrange
            var employees = this.fixture.CreateMany<Employee>(10);

            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Throws(new Exception("Insufficient funds to pay employees."));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(employees);

            var sut = this.fixture.Create<IPayrollRunner>();
            
            // Act
            Action act = () => sut.RunPayroll();

            // Assert
            act.Should().Throw<Exception>().WithMessage("Insufficient funds to pay employees.");
        }

        [Fact]
        public void RunPayroll_WithEmployees_ExternalProviderReturnsErrors()
        {
            // Arrange
            var employees = this.fixture.CreateMany<Employee>(10);

            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>
                {
                    new PayrollProviderResult
                    {
                        EmployeeId = employees.Last().Id,
                        FailureCode = 4521,
                        FailureReason = "Bank account not found"
                    }
                }));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(employees);
            
            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            var result = sut.RunPayroll();

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(It.IsAny<string>()), Times.Once());

            result.Errors.Should().HaveCount(1);
        }

        [Fact]
        public void RunPayroll_WithDeductions_WillReduceNetPay()
        {
            // Arrange
            var expectedPayroll = new List<dynamic>
            {
                new
                {
                    EmployeeId = 1,
                    NetPay = 2250.0m
                }
            };
            var expectedJson = JsonConvert.SerializeObject(expectedPayroll);

            var employee = this.fixture
                .Build<Employee>()
                .With(x => x.Id, 1)
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, "Thorpe")
                .With(x => x.GrossPay, 2500m)
                .With(x => x.Deductions, new List<Deduction>
                {
                    new Deduction
                    {
                        Description = "Pension",
                        Percentage = 10
                    }
                })
                .Create();

            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(expectedJson))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(new List<Employee> { employee });
            
            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            var result = sut.RunPayroll();

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(expectedJson), Times.Once());
            
            result.Errors.Should().BeNull();
        }

        [Fact]
        public void RunPayroll_WithManyDeductions_WillReduceNetPayAccordingly()
        {
            // Arrange
            var expectedPayroll = new List<dynamic>
            {
                new
                {
                    EmployeeId = 1,
                    NetPay = 2250.0m
                },
                new
                {
                    EmployeeId = 2,
                    NetPay = 2100.00m
                }
            };
            var expectedJson = JsonConvert.SerializeObject(expectedPayroll);

            var employee = this.fixture
                .Build<Employee>()
                .With(x => x.Id, 1)
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, "Thorpe")
                .With(x => x.GrossPay, 2500m)
                .With(x => x.Deductions, new List<Deduction>
                {
                    new Deduction
                    {
                        Description = "Pension",
                        Percentage = 10
                    }
                })
                .Create();

            var employeeWithStudentLoan = this.fixture
                .Build<Employee>()
                .With(x => x.Id, 2)
                .With(x => x.FirstName, "Harry")
                .With(x => x.LastName, "Gold")
                .With(x => x.GrossPay, 2500m)
                .With(x => x.Deductions, new List<Deduction>
                {
                    new Deduction
                    {
                        Description = "Pension",
                        Percentage = 10
                    },
                    new Deduction
                    {
                        Description = "Student Loan",
                        Percentage = 6
                    }
                })
                .Create();

            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Setup(x => x.RunPayroll(expectedJson))
                .Returns(JsonConvert.SerializeObject(new List<PayrollProviderResult>()));

            this.fixture
                .Create<Mock<IRepository<Employee>>>()
                .Setup(x => x.GetAll())
                .Returns(new List<Employee> { employee, employeeWithStudentLoan });
            
            // Act
            var sut = this.fixture.Create<IPayrollRunner>();
            var result = sut.RunPayroll();

            // Assert
            this.fixture
                .Create<Mock<IExternalPayrollProvider>>()
                .Verify(x => x.RunPayroll(expectedJson), Times.Once());
            
            result.Errors.Should().BeNull();
        }
    }
}
