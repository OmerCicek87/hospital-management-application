namespace Hospital_Management.Entities;

public class Nurse : Employee
{
    
    public Nurse() { }
    
    public Nurse(string name, string surname, string pesel, string username, string password)
        : base(name, surname, pesel, username, password) {}
}