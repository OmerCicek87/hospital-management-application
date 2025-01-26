using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Hospital_Management.Data;
using Hospital_Management.DTO;
using Hospital_Management.Entities;

namespace Hospital_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (loginDto == null ||
                string.IsNullOrEmpty(loginDto.UserName) ||
                string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Username and password are required.");
            }
            
            var user = _context.Employees.FirstOrDefault(e => e.UserName == loginDto.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password.");
            
            string roleName;
            if (user is Administrator)
                roleName = "Admin";
            else if (user is Doctor)
                roleName = "Doctor";
            else if (user is Nurse)
                roleName = "Nurse";
            else
                return BadRequest("Unknown role");
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, roleName)
            };
            
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in using the cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true
                }
            );
            
            return Ok(roleName);
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logged out successfully");
        }
        
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userName = User.Identity.Name;
            var employee = _context.Employees.SingleOrDefault(e => e.UserName == userName);
            if(employee == null) return NotFound();

            if(employee is Doctor doc) {
                return Ok(new {
                    doc.Id,
                    doc.Name,
                    doc.Surname,
                    doc.PWZNumber,
                    doc.Specialty
                });
            }
            return Ok(new {
                employee.Id,
                employee.Name,
                employee.Surname
            });
        }

    }
}