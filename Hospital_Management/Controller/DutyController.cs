using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hospital_Management.Data;
using Hospital_Management.Entities;

namespace Hospital_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DutyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DutyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all duties
        [HttpGet]
        public ActionResult<IEnumerable<Duty>> GetAllDuties()
        {
            var duties = _context.Duties
                .Select(d => new
                {
                    d.Id,
                    EmployeeName = d.Employee.Name,
                    EmployeeSurname = d.Employee.Surname,
                    DutyDate = d.DutyDate.ToString("yyyy-MM-dd"),
                    d.StartHour,
                    d.EndHour
                })
                .ToList();
            return Ok(duties);
        }

        // Get duties for a specific employee
        [HttpGet("employee/{employeeId}")]
        public ActionResult<IEnumerable<Duty>> GetDutiesForEmployee(int employeeId)
        {
            var duties = _context.Duties
                .Where(d => d.Employee.Id == employeeId)
                .Select(d => new
                {
                    d.Id,
                    DutyDate = d.DutyDate.ToString("yyyy-MM-dd"), // Format the date
                    d.StartHour,
                    d.EndHour
                })
                .ToList();
            return Ok(duties);
        }
        
        // Create a new duty
        [HttpPost("duties")]
        public IActionResult CreateDuty([FromBody] DutyCreateDTO dto)
        {
            var employee = _context.Employees.Find(dto.EmployeeId);
            if (employee == null) return NotFound("Employee not found.");

            var duty = new Duty
            {
                Employee = employee,
                DutyDate = dto.DutyDate,
                StartHour = dto.StartHour,
                EndHour = dto.EndHour
            };

            _context.Duties.Add(duty);
            _context.SaveChanges();

            return Ok(new
            {
                duty.Id,
                EmployeeName = employee.Name,
                EmployeeSurname = employee.Surname,
                duty.DutyDate,
                duty.StartHour,
                duty.EndHour
            });
        }

        // Update an existing duty
        [HttpPut("{dutyId}")]
        public IActionResult UpdateDuty(int dutyId, [FromBody] DutyUpdateDTO dto)
        {
            var duty = _context.Duties.Find(dutyId);
            if (duty == null)
                return NotFound($"Duty with ID {dutyId} not found.");

            if (dto.DutyDate.HasValue)
                duty.DutyDate = dto.DutyDate.Value;
            if (dto.StartHour.HasValue)
                duty.StartHour = dto.StartHour.Value;
            if (dto.EndHour.HasValue)
                duty.EndHour = dto.EndHour.Value;

            _context.SaveChanges();
            return NoContent();
        }

        // Delete a duty
        [HttpDelete("{dutyId}")]
        public IActionResult DeleteDuty(int dutyId)
        {
            var duty = _context.Duties.Find(dutyId);
            if (duty == null)
                return NotFound($"Duty with ID {dutyId} not found.");

            _context.Duties.Remove(duty);
            _context.SaveChanges();
            return NoContent();
        }

        // Report a duty problem
        [AllowAnonymous]
        [HttpPost("report")]
        public IActionResult ReportDutyProblem([FromBody] DutyReportCreateDTO dto)
        {
            var duty = _context.Duties.Find(dto.DutyId);
            if (duty == null)
                return NotFound($"Duty with ID {dto.DutyId} not found.");

            var report = new DutyReport
            {
                DutyId = dto.DutyId,
                ReportingEmployeeId = dto.ReportingEmployeeId,
                Message = dto.Message
            };
            _context.DutyReports.Add(report);
            _context.SaveChanges();
            return Ok(report);
        }

        // Get all reports
        [HttpGet("reports")]
        public IActionResult GetAllReports()
        {
            var reports = _context.DutyReports
                .Select(r => new
                {
                    r.Id,
                    r.DutyId,
                    DutyEmployeeName = r.Duty.Employee.Name,
                    DutyEmployeeSurname = r.Duty.Employee.Surname,
                    r.Message,
                    r.IsResolved
                })
                .ToList();
            return Ok(reports);
        }

        // Resolve a report
        [HttpPut("reports/{reportId}/resolve")]
        public IActionResult ResolveReport(int reportId)
        {
            var report = _context.DutyReports.Find(reportId);
            if (report == null)
                return NotFound($"Report ID {reportId} not found.");

            report.IsResolved = true;
            _context.SaveChanges();
            return NoContent();
        }

        // Delete a report
        [HttpDelete("reports/{reportId}")]
        public IActionResult DeleteReport(int reportId)
        {
            var report = _context.DutyReports.Find(reportId);
            if (report == null) return NotFound("Report not found");

            _context.DutyReports.Remove(report);
            _context.SaveChanges();
            return NoContent();
        }
    }

    // DTO for creating a duty
    public class DutyCreateDTO
    {
        public int EmployeeId { get; set; }
        public DateTime DutyDate { get; set; }
        public TimeSpan StartHour { get; set; }
        public TimeSpan EndHour { get; set; }
    }

    // DTO for updating a duty
    public class DutyUpdateDTO
    {
        public DateTime? DutyDate { get; set; }
        public TimeSpan? StartHour { get; set; }
        public TimeSpan? EndHour { get; set; }
    }

    // DTO for creating a duty report
    public class DutyReportCreateDTO
    {
        public int DutyId { get; set; }
        public int ReportingEmployeeId { get; set; }
        public string Message { get; set; }
    }
}
