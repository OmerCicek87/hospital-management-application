using Hospital_Management.Controllers;
using Hospital_Management.Data;
using Hospital_Management.DTO;
using Hospital_Management.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Tests.Controllers
{
    [TestFixture]
    public class AdminControllerTests
    {
        private ApplicationDbContext _context;
        private AdminController _controller;
        
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            
            _context.Employees.AddRange(
                new Administrator
                {
                    Name = "Admin1",
                    Surname = "Test",
                    RoleId = 1,
                    PESEL = "12345678901",
                    UserName = "admin1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
                },
                new Doctor
                {
                    Name = "Doctor1",
                    Surname = "Test",
                    RoleId = 2,
                    Specialty = Specialty.Cardiologist,
                    PWZNumber = "12345",
                    PESEL = "98765432101",
                    UserName = "doctor1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password456")
                },
                new Nurse
                {
                    Name = "Nurse1",
                    Surname = "Test",
                    RoleId = 3,
                    PESEL = "45678912301",
                    UserName = "nurse1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password789")
                }
            );
            _context.SaveChanges();
            
            _controller = new AdminController(_context);
        }


        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // Test Success
        [Test]
        public void GetAllEmployees_ReturnsAllEmployees()
        {
            var result = _controller.GetAllEmployees();
            
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOf<List<Employee>>(okResult.Value);

            var employees = (List<Employee>)okResult.Value;
            Assert.AreEqual(3, employees.Count);
        }

        // Test Success
        [Test]
        public void GetEmployeeById_ValidId_ReturnsEmployee()
        {
            var existingEmployee = _context.Employees.First();
            
            var result = _controller.GetEmployeeById(existingEmployee.Id);
            
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = (OkObjectResult)result.Result;
            Assert.IsInstanceOf<Employee>(okResult.Value);

            var employee = (Employee)okResult.Value;
            Assert.AreEqual(existingEmployee.Name, employee.Name);
        }
        
        // Test Success
        [Test]
        public void GetEmployeeById_InvalidId_ReturnsNotFound()
        {
            var result = _controller.GetEmployeeById(999);
            
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        // Test Success
        [Test]
        public void AddEmployee_ValidData_CreatesEmployee()
        {
            var dto = new EmployeeCreateDTO
            {
                Name = "NewDoctor",
                Surname = "Test",
                PESEL = "98765432101",
                UserName = "new_doctor",
                Password = "password123",
                Discriminator = "Doctor",
                RoleId = 2,
                Specialty = (int)Specialty.Neurologist,
                PwzNumber = "67890"
            };
            
            var result = _controller.AddEmployee(dto);

            
            Assert.IsInstanceOf<CreatedAtActionResult>(result);

            var createdResult = (CreatedAtActionResult)result;
            Assert.IsInstanceOf<Doctor>(createdResult.Value);

            var employee = (Doctor)createdResult.Value;
            Assert.AreEqual("NewDoctor", employee.Name);
            Assert.AreEqual(4, _context.Employees.Count());
        }
        
        // Test Success
        [Test]
        public void AddEmployee_InvalidRoleId_ReturnsBadRequest()
        {
            var dto = new EmployeeCreateDTO
            {
                Name = "InvalidEmployee",
                Surname = "Test",
                PESEL = "12345678901",
                UserName = "invalid_employee",
                Password = "password",
                RoleId = 99
            };
            
            var result = _controller.AddEmployee(dto);
            
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        // Test Success
        [Test]
        public void UpdateEmployee_ValidData_UpdatesEmployee()
        {
            var existingEmployee = _context.Employees.OfType<Doctor>().First();
            var dto = new EmployeeUpdateDTO
            {
                Name = "UpdatedName",
                Surname = "UpdatedSurname",
                Specialty = (int)Specialty.Laryngologist
            };
            
            var result = _controller.UpdateEmployee(existingEmployee.Id, dto);
            
            Assert.IsInstanceOf<NoContentResult>(result);
            var updatedEmployee = _context.Employees.OfType<Doctor>().First();
            Assert.AreEqual("UpdatedName", updatedEmployee.Name);
            Assert.AreEqual(Specialty.Laryngologist, updatedEmployee.Specialty);
        }

        // Test Success
        [Test]
        public void UpdateEmployee_InvalidId_ReturnsNotFound()
        {
            var dto = new EmployeeUpdateDTO { Name = "Invalid" };
            var result = _controller.UpdateEmployee(999, dto);
            
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        // Test Success
        [Test]
        public void DeleteEmployee_ValidId_DeletesEmployee()
        {
            var existingEmployee = _context.Employees.First();
            var result = _controller.DeleteEmployee(existingEmployee.Id);
            
            Assert.IsInstanceOf<NoContentResult>(result);
            Assert.AreEqual(2, _context.Employees.Count());
        }

        // Test Success
        [Test]
        public void DeleteEmployee_InvalidId_ReturnsNotFound()
        {
            var result = _controller.DeleteEmployee(999);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }
}
