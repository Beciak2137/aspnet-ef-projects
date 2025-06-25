namespace APBD10.DTO;

public class TripGetDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryGetDto> Countries { get; set; } = null!;
    public List<ClientGetDto> Clients { get; set; } = null!;
}