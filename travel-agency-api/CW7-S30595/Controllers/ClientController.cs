using CW7_S30595.Exceptions;
using CW7_S30595.Models;
using CW7_S30595.Models.DTOs;
using CW7_S30595.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW7_S30595.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController(IDbService DbService) : ControllerBase
{
    //zwróć liste wycieczek danego klienta
    [HttpGet]
    [Route("{id}/trips")]
    public async Task<IActionResult> GetClientTrips([FromRoute] int id)
    {
        try
        {
            return Ok(await DbService.GetClientTripsAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    //dodawanie klienta
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO clientCreateDto)
    {
        var client = await DbService.CreateClientAsync(clientCreateDto);
        return Created($"clients/{client.IdClient}", client);
    }
    //zarejestruj klienta na wycieczke
    [HttpPut]
    [Route("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientForTrip([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            await DbService.RegisterClientForTripAsync(id, tripId);
            return Ok("Client registered for trip");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (DifferentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    [Route("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientFromTrip([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            await DbService.DeleteClientFromTripAsync(id, tripId);
            return Ok("Client deleted from trip");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
}