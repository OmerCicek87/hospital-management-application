using Hospital_Management.Entities;

public class Administrator : Employee
{
    public Administrator() { }
    
    public Administrator(string name, string surname, string pesel, string username, string password)
        : base(name, surname, pesel, username, password) { }
}