namespace Hospital_Management.DTO
{
    public class EmployeeUpdateDTO
    {
        // Common fields
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? PESEL { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        
        public int? RoleId { get; set; }

        // Doctor-specific fields
        public int? Specialty { get; set; }
        public string? PwzNumber { get; set; }
    }
}