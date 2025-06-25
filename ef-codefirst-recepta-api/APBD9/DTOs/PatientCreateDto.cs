using System.ComponentModel.DataAnnotations;

namespace APBD9.DTOs;

public class PatientCreateDto
{
    public int IdPatient { get; set; }
    
    [MaxLength(100)]
    public string FirstName { get; set; }
    
    [MaxLength(100)]
    public string LastName { get; set; }
    
    public DateTime BirthDate { get; set; }
}