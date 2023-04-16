using HotelServices.Domain.Entities;
using HotelServices.Domain.Interfaces;
using HotelServices.WebAPI.Filters;

using Microsoft.AspNetCore.Mvc;

namespace HotelServices.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AdditionalServicesController : ControllerBase
{
    private readonly IRepository<AdditionalService> _repository;

    public AdditionalServicesController(IRepository<AdditionalService> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var additionalServices = await _repository.GetAllAsync();
        return Ok(additionalServices);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var additionalService = await _repository.GetByIdAsync(id);
        if (additionalService == null)
        {
            return NotFound();
        }

        return Ok(additionalService);
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Create(AdditionalService additionalService)
    {
        await _repository.CreateAsync(additionalService);
        return CreatedAtAction(nameof(GetById), new { id = additionalService.Id }, additionalService);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AdditionalService additionalService)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(id, additionalService);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }
}