namespace APBD9.DTOs;

public class PrescriptionCreateDto
{
    public PatientGetDto Patient { get; set; }
    public List<MedicamentGetDto> Medicaments { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int IdDoctor { get; set; }
    public int IdPatient { get; set; }
}