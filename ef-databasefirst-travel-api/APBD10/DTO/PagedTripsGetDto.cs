namespace APBD10.DTO;

public class PagedTripsGetDto
{
    public int? PageNum { get; set; }
    public int? PageSize { get; set; }
    public int? AllPages { get; set; }
    public List<TripGetDto> Trips { get; set; } = null!;
}