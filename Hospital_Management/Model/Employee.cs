namespace Hospital_Management.Entities;

public abstract class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string PESEL { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public int? RoleId { get; set; }
    public Role? Role { get; set; }
    public ICollection<Duty>? Duties { get; set; }
    
    protected Employee() { }

    // Base constructor for creating employee objects
    protected Employee(string name, string surname, string pesel, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(surname)) throw new ArgumentException("Surname cannot be empty.");
        if (string.IsNullOrWhiteSpace(pesel) || pesel.Length != 11) throw new ArgumentException("Invalid PESEL.");
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty.");

        Name = name;
        Surname = surname;
        PESEL = pesel;
        UserName = username;
        PasswordHash = HashPassword(password);
        Duties = new List<Duty>();
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}