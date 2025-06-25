using APBD10.Data;
using APBD10.DTO;
using APBD10.Exceptions;
using APBD10.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int? pageNum, [FromQuery] int? pageSize)
    {
        try
        {
            if (pageNum != null)
            {
                return Ok(await dbService.GetTripsAsync(pageNum.Value, pageSize));
            }
            return Ok(await dbService.GetTripsAsync());
        }
        catch (BadRequestException e)
        {
            return BadRequest(e.Message);
        }
    }
    [HttpPost("{tripId}/clients")]
    public async Task<IActionResult> AssignClientForTrip([FromRoute] int tripId, [FromBody] ClientPostDto client)
    {
        try
        {
            await dbService.AssignClientForTripAsync(tripId, client);
            return Ok();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (BadRequestException e)
        {
            return Conflict(e.Message);
        }
    }
    
}