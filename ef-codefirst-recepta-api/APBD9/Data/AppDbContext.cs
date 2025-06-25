using APBD9.Models;
using Microsoft.EntityFrameworkCore;


namespace APBD9.Data;

public class AppDbContext : DbContext
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Prescription_Medicament> Prescription_Medicaments { get; set; }

    protected AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                IdDoctor = 1,
                FirstName = "AAA",
                LastName = "Daniel",
                Email = "daniel.daniel@gmail.com",
            },
            new Doctor
            {
                IdDoctor = 2,
                FirstName = "James",
                LastName = "James",
                Email = "james.james@gmail.com",
            }
        );
        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                IdPatient = 1,
                FirstName = "Jan",
                LastName = "Daniel",
                BirthDate = new DateTime(1980, 1, 1),
            },
            new Patient
            {
                IdPatient = 2,
                FirstName = "Agata",
                LastName = "James",
                BirthDate = new DateTime(1980, 1, 1),
            }
        );
        modelBuilder.Entity<Medicament>().HasData(
            new Medicament
            {
                IdMedicament = 1,
                Name = "AAA",
                Description = "AAA",
                Type = "Medicament",
            },
            new Medicament
            {
                IdMedicament = 2,
                Name = "Apap",
                Description = "des..",
                Type = "Medicament",
            }
        );
        modelBuilder.Entity<Prescription>().HasData(
            new Prescription()
            {
                IdPrescription = 1,
                Date = new DateTime(2012,01,01),
                DueDate = new DateTime(2012,01,01),
                IdPatient = 1,
                IdDoctor = 1,
            }
        );
        modelBuilder.Entity<Prescription_Medicament>().HasData(
            new Prescription_Medicament()
            {
                IdPrescription = 1,
                IdMedicament = 1,
                Dose = 10,
                Details = "AAA"
            }
        );
        
    }
}