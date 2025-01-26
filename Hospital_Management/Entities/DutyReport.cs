namespace Hospital_Management.Entities;

public class DutyReport
{
    public int Id { get; set; }
    public int DutyId { get; set; }
    public Duty Duty { get; set; }

    public int ReportingEmployeeId { get; set; }
    public Employee ReportingEmployee { get; set; }

    public string Message { get; set; }
    
    public bool IsResolved { get; set; } = false;
}

