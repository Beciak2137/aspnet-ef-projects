using APBD9.Data;
using APBD9.DTOs;
using APBD9.Exceptions;
using APBD9.Models;
using APBD9.Service;
using Microsoft.AspNetCore.Mvc;

namespace APBD9.Controllers;

[ApiController]
[Route("[controller]")]
public class PatientController : ControllerBase
{
    private readonly IDbService _dbService;

    public PatientController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientDetails([FromRoute] int id)
    {
        try
        {
            return Ok(await _dbService.GetPatientDetailsAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPrescription([FromBody] PrescriptionCreateDto prescriptionCreate)
    {
        try
        {
            return Ok(prescriptionCreate);

        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (BadRequest e)
        {
            return BadRequest(e.Message);
        }
    }
    
}