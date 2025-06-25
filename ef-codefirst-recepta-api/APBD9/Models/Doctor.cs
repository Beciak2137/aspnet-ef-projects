using System.ComponentModel.DataAnnotations;

namespace APBD9.Models;

public class Doctor
{
    [Key]
    public int IdDoctor { get; set; }
    
    [MaxLength(100)]
    public string FirstName { get; set; }
    
    [MaxLength(100)]
    public string LastName { get; set; }
    
    [MaxLength(100)]
    public string Email { get; set; }
    
    public virtual ICollection<Prescription> Prescriptions { get; set; } = null!;
}