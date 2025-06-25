using APBD10.Exceptions;
using APBD10.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IDbService dbService) : ControllerBase
{
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClientById([FromRoute] int id)
    {
        try
        {
            await dbService.DeleteClientByIdAsync(id);
            return NoContent();
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