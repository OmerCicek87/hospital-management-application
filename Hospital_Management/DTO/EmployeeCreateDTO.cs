using Hospital_Management.Entities;

namespace Hospital_Management.DTO
{
    public class EmployeeCreateDTO
    {
        // Common fields
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PESEL { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        
        public string? Discriminator { get; set; }
        public int? RoleId { get; set; }

        // Doctor-specific fields
        public int? Specialty { get; set; }
        public string? PwzNumber { get; set; }
    }
}