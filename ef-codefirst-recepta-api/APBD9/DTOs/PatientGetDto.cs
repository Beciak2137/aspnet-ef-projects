namespace APBD9.DTOs;

public class PatientGetDto
{
    public int IdPatient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string BirthDate { get; set; }
    public List<PrescriptionGetDto>? Prescriptions { get; set; }
}


