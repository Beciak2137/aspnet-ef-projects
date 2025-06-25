using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APBD9.Models;
[PrimaryKey(nameof(IdMedicament),nameof(IdPrescription))]
public class Prescription_Medicament
{
    public int IdMedicament { get; set; }
    [ForeignKey("IdMedicament")]
    public Medicament Medicament { get; set; }
    
    public int IdPrescription { get; set; }
    [ForeignKey("IdPrescription")]
    public Prescription prescription { get; set; }
    
    public int? Dose { get; set; }
    [MaxLength(100)]
    public string Details { get; set; }
}