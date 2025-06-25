using CW7_S30595.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW7_S30595.Controllers;

[ApiController]
[Route("[controller]")]
public class TripController(IDbService DbService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetTrips()
    {
        return Ok(await DbService.GetTripsAsync());
    }
    
}