using APBD9.Data;
using APBD9.DTOs;
using APBD9.Exceptions;
using APBD9.Models;
using Microsoft.EntityFrameworkCore;


namespace APBD9.Service;

public interface IDbService
{
    public Task<PatientGetDto> GetPatientDetailsAsync(int patientId);
    public Task<PrescriptionCreateDto> AddPrescriptionAsync(PrescriptionCreateDto prescriptionCreate);
}

public class DbService(AppDbContext data) : IDbService
{
    public async Task<PatientGetDto> GetPatientDetailsAsync(int patientId)
    {
        var patient = await data.Patients.FirstOrDefaultAsync(p => p.IdPatient == patientId);

    if (patient == null)
        throw new NotFoundException($"Patient with id: {patientId} not found");
    
    var prescriptions = await data.Prescriptions
        .Where(pr => pr.IdPatient == patientId)
        .OrderBy(pr => pr.DueDate)
        .ToListAsync();
    
    var doctorIds = prescriptions.Select(p => p.IdDoctor).Distinct().ToList();
    var doctors = await data.Doctors
        .Where(d => doctorIds.Contains(d.IdDoctor))
        .ToDictionaryAsync(d => d.IdDoctor);

    var prescriptionIds = prescriptions.Select(p => p.IdPrescription).ToList();
    var prescriptionMedicaments = await data.Prescription_Medicaments
        .Where(pm => prescriptionIds.Contains(pm.IdPrescription))
        .ToListAsync();

    var medicamentIds = prescriptionMedicaments.Select(pm => pm.IdMedicament).Distinct().ToList();
    var medicaments = await data.Medicaments
        .Where(m => medicamentIds.Contains(m.IdMedicament))
        .ToDictionaryAsync(m => m.IdMedicament);
    
    var dto = new PatientGetDto
    {
        IdPatient = patient.IdPatient,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        BirthDate = patient.BirthDate.ToString("yyyy-MM-dd"),
        Prescriptions = prescriptions.Select(p => new PrescriptionGetDto
        {
            IdPrescription = p.IdPrescription,
            Date = p.Date.ToString("yyyy-MM-dd"),
            DueDate = p.DueDate.ToString("yyyy-MM-dd"),

            DoctorGet = new DoctorGetDto
            {
                IdDoctor = doctors[p.IdDoctor].IdDoctor,
                FirstName = doctors[p.IdDoctor].FirstName
            },
            Medicaments = prescriptionMedicaments
                .Where(pm => pm.IdPrescription == p.IdPrescription)
                .Select(pm => new MedicamentGetDto
                {
                    IdMedicament = pm.IdMedicament,
                    Name = medicaments[pm.IdMedicament].Name,
                    Description = medicaments[pm.IdMedicament].Description,
                    Dose = pm.Dose
                }).ToList()
        }).ToList()
    };

    return dto;
    }

    public async Task<PrescriptionCreateDto> AddPrescriptionAsync(PrescriptionCreateDto prescriptionCreate)
    {
        if (prescriptionCreate.DueDate < prescriptionCreate.DueDate)
        {
            throw new BadHttpRequestException("DueDate must be greater than or equal to Date.");
        }

        if (prescriptionCreate.Medicaments.Count > 10)
        {
            throw new BadHttpRequestException("Medicaments count must be less than 10.");
        }
        await using var transaction = await data.Database.BeginTransactionAsync();
        try
        {
            var doctor = await data.Doctors.FirstOrDefaultAsync(d => d.IdDoctor == prescriptionCreate.IdDoctor)
                        ?? throw new NotFoundException($"Doctor with id: {prescriptionCreate.IdDoctor} not found");
            
            var patient = await data.Patients.FirstOrDefaultAsync(p => p.IdPatient == prescriptionCreate.IdPatient);

            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = prescriptionCreate.Patient.FirstName,
                    LastName = prescriptionCreate.Patient.LastName,
                    BirthDate = Convert.ToDateTime(prescriptionCreate.Patient.BirthDate)
                };

                await data.Patients.AddAsync(patient);
                await data.SaveChangesAsync();
            }
            prescriptionCreate.IdPatient = patient.IdPatient;
            var prescription = new Prescription
            {
                Date = prescriptionCreate.Date,
                DueDate = prescriptionCreate.DueDate,
                IdDoctor = doctor.IdDoctor,
                IdPatient = patient.IdPatient
            };

            await data.Prescriptions.AddAsync(prescription);
            await data.SaveChangesAsync();
            
            foreach (var medicament in prescriptionCreate.Medicaments)
            {
                var exists = await data.Medicaments.AnyAsync(m => m.IdMedicament == medicament.IdMedicament);
                if (!exists)
                    throw new NotFoundException($"Medicament with id: {medicament.IdMedicament} not found");

                await data.Prescription_Medicaments.AddAsync(new Prescription_Medicament
                {
                    IdPrescription = prescription.IdPrescription,
                    IdMedicament = medicament.IdMedicament,
                    Dose = medicament.Dose,
                    Details = medicament.Description
                });
            }

            await data.SaveChangesAsync();
            await transaction.CommitAsync();

            return prescriptionCreate;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}