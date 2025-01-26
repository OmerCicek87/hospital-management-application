using Microsoft.AspNetCore.Mvc;
using Hospital_Management.Data;
using Hospital_Management.Entities;
using Hospital_Management.DTO;
using Microsoft.AspNetCore.Authorization;

namespace Hospital_Management.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]

public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context) {  _context = context;  }

    // Get all employees
    [HttpGet("employees")]
    public ActionResult<List<object>> GetAllEmployees()
    {
        try
        {
            var employees = _context.Employees
                .Select(e => new 
                {
                    e.Id,
                    e.Name,
                    e.Surname,
                    e.PESEL,
                    e.UserName,
                    Discriminator = e is Administrator ? "Administrator" :
                        e is Doctor ? "Doctor" :
                        e is Nurse ? "Nurse" : null,
                    Specialty = e is Doctor ? ((Doctor)e).Specialty.ToString() : null // Cast explicitly
                })
                .ToList();

            if (employees == null || employees.Count == 0)
                return NotFound("No employees found.");

            return Ok(employees);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    // Get employee by ID
    [HttpGet("employees/{id}")]
    public ActionResult<Employee> GetEmployeeById(int id)
    {
        try
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound($"Employee with ID {id} not found.");
            }
            return Ok(employee);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("employees")]
    public IActionResult AddEmployee([FromBody] EmployeeCreateDTO dto)
{
    try
    {
        if (!string.IsNullOrEmpty(dto.Discriminator) && dto.RoleId.HasValue)
        {
            var expectedDisc = DiscriminatorFromRoleId(dto.RoleId.Value);
            if (expectedDisc == null)
                return BadRequest($"Invalid roleId '{dto.RoleId.Value}'. Must be 1, 2, or 3.");
            
            if (!dto.Discriminator.Equals(expectedDisc, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"Mismatch: roleId {dto.RoleId.Value} corresponds to '{expectedDisc}', but 'Discriminator' was '{dto.Discriminator}'.");
        }
        else if (dto.RoleId.HasValue && string.IsNullOrEmpty(dto.Discriminator))
        {
            var disc = DiscriminatorFromRoleId(dto.RoleId.Value);
            if (disc == null)
                return BadRequest($"Invalid roleId '{dto.RoleId.Value}'. Must be 1, 2, or 3.");

            dto.Discriminator = disc;
        }
        else if (!string.IsNullOrEmpty(dto.Discriminator) && !dto.RoleId.HasValue)
        {
            var rId = RoleIdFromDiscriminator(dto.Discriminator);
            if (!rId.HasValue)
                return BadRequest($"Invalid Discriminator '{dto.Discriminator}' (must be 'Administrator', 'Doctor', or 'Nurse').");

            dto.RoleId = rId.Value;
        }
        else if (string.IsNullOrEmpty(dto.Discriminator) && !dto.RoleId.HasValue)
            return BadRequest("Discriminator or RoleId must be provided.");

        Employee employee;
        switch (dto.Discriminator)
        {
            case "Administrator":
                employee = new Administrator(
                    dto.Name,
                    dto.Surname,
                    dto.PESEL,
                    dto.UserName,
                    dto.Password
                )
                {
                    RoleId = dto.RoleId.Value
                };
                break;

            case "Doctor":
                if (!dto.Specialty.HasValue)
                    return BadRequest("Missing 'Specialty' for Doctor.");
                if (string.IsNullOrEmpty(dto.PwzNumber))
                    return BadRequest("Missing 'PwzNumber' for Doctor.");

                employee = new Doctor(
                    dto.Name,
                    dto.Surname,
                    dto.PESEL,
                    dto.UserName,
                    dto.Password,
                    (Specialty)dto.Specialty.Value,
                    dto.PwzNumber
                )
                {
                    RoleId = dto.RoleId.Value
                };
                break;

            case "Nurse":
                employee = new Nurse(
                    dto.Name,
                    dto.Surname,
                    dto.PESEL,
                    dto.UserName,
                    dto.Password
                )
                {
                    RoleId = dto.RoleId.Value
                };
                break;

            default:
                return BadRequest("Invalid discriminator (must be 'Administrator', 'Doctor', or 'Nurse').");
        }
        
        if (employee.Duties == null)
            employee.Duties = new List<Duty>();
        
        // Save changes to the database
        _context.Employees.Add(employee);
        _context.SaveChanges();

        // Return the created employee
        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return StatusCode(500, "Internal server error");
    }
}
    
    // Update an existing employee
    [HttpPut("employees/{id}")]
    public IActionResult UpdateEmployee(int id, [FromBody] EmployeeUpdateDTO updateDto)
    {
        try
        {
            // Find existing employee by id
            var existingEmployee = _context.Employees.Find(id);
            if (existingEmployee == null)
                return NotFound($"Employee with ID {id} not found.");

            // Update given fields
            if (!string.IsNullOrEmpty(updateDto.Name))
                existingEmployee.Name = updateDto.Name;

            if (!string.IsNullOrEmpty(updateDto.Surname))
                existingEmployee.Surname = updateDto.Surname;

            if (!string.IsNullOrEmpty(updateDto.PESEL))
                existingEmployee.PESEL = updateDto.PESEL;

            if (!string.IsNullOrEmpty(updateDto.UserName))
                existingEmployee.UserName = updateDto.UserName;

            if (!string.IsNullOrEmpty(updateDto.Password))
                existingEmployee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);

            if (updateDto.RoleId.HasValue)
                existingEmployee.RoleId = updateDto.RoleId.Value;

            // Check if the existing employee is a Doctor
            if (existingEmployee is Doctor existingDoctor)
            {
                if (updateDto.Specialty.HasValue)
                    existingDoctor.Specialty = (Specialty)updateDto.Specialty.Value;

                if (!string.IsNullOrEmpty(updateDto.PwzNumber))
                    existingDoctor.PWZNumber = updateDto.PwzNumber;
            }
        
            // Save changes to the database
            _context.SaveChanges();

            // Return "204 No Content" or "200 OK"
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    // Delete an employee
    [HttpDelete("employees/{id}")]
    public IActionResult DeleteEmployee(int id)
    {
        try
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound($"Employee with ID {id} not found.");
            }

            _context.Employees.Remove(employee);
            _context.SaveChanges();

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    private static string? DiscriminatorFromRoleId(int roleId)
    {
        return roleId switch
        {
            1 => "Administrator",
            2 => "Doctor",
            3 => "Nurse",
            _ => null
        };
    }

    private static int? RoleIdFromDiscriminator(string discriminator)
    {
        return discriminator.ToLower() switch
        {
            "administrator" => 1,
            "doctor"        => 2,
            "nurse"         => 3,
            _               => null
        };
    }
}
