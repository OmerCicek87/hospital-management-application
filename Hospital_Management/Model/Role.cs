namespace Hospital_Management.Entities;

public class Role
{
    
    public int Id { get; set; }
    public string RoleName { get; set; }
    public ICollection<Employee> Employees { get; set; }
}