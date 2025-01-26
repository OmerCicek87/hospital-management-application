using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hospital_Management.Entities;

namespace Hospital_Management.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { } 
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Duty> Duties { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<DutyReport> DutyReports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity columns for MySQL compatibility
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Find all properties named 'Id' that are of type TEXT and set them to VARCHAR(255)
            foreach (var property in entityType.GetProperties())
            {
                if (property.Name == "Id" && property.ClrType == typeof(string))
                {
                    property.SetColumnType("varchar(255)");
                }
            }
        }

        // Configurations
        modelBuilder.Entity<Administrator>();
        modelBuilder.Entity<Doctor>();
        modelBuilder.Entity<Nurse>();

        // Configure Employee <--> Role Relation
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Role)
            .WithMany(r => r.Employees)
            .HasForeignKey(e => e.RoleId);

        // Configure Employee <--> Duties Relation
        modelBuilder.Entity<Duty>()
            .HasOne(d => d.Employee)
            .WithMany(e => e.Duties)
            .HasForeignKey(d => d.EmployeeId);

        // Discriminator configuration
        modelBuilder.Entity<Employee>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Administrator>("Admin")
            .HasValue<Doctor>("Doctor")
            .HasValue<Nurse>("Nurse");
    }



}
