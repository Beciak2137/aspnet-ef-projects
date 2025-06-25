using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APBD9.Models;

public class Prescription
{
    [Key]
    public int IdPrescription { get; set; }
    
    public DateTime Date { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public int IdPatient { get; set; }
    [ForeignKey("IdPatient")]
    public Patient Patient { get; set; }
    
    public int IdDoctor { get; set; }
    [ForeignKey("IdDoctor")]
    public Doctor Doctor { get; set; }

    public virtual ICollection<Prescription_Medicament> PRE { get; set; } = null!;
}