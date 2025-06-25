using System.ComponentModel.DataAnnotations;

namespace APBD10.DTO;

public class ClientPostDto
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string LastName { get; set; } = null!;
    
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Email { get; set; } = null!;
    
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Telephone { get; set; } = null!;
    
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Pesel { get; set; } = null!;
    
    public DateTime? PaymentDate { get; set; }
}