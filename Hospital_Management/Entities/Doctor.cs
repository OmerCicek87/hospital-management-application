namespace Hospital_Management.Entities;

public enum Specialty
{
    Cardiologist = 1,
    Urologist,
    Neurologist,
    Laryngologist,
}

public class Doctor : Employee
{
    public Specialty Specialty {get; set;} 
    public string PWZNumber {get; set;}
    
    public Doctor() { }
    
    public Doctor(
        string name,
        string surname,
        string pesel,
        string username,
        string password,
        Specialty specialty,
        string pwzNumber
    ) : base(name, surname, pesel, username, password)
    {
        Specialty = specialty;
        PWZNumber = pwzNumber;
    }
}
