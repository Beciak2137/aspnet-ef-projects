namespace APBD9.DTOs;

public class PrescriptionGetDto
{
    public int IdPrescription { get; set; }
    public string Date { get; set; }     
    public string DueDate { get; set; }
    public List<MedicamentGetDto> Medicaments { get; set; }
    public DoctorGetDto DoctorGet { get; set; }
}