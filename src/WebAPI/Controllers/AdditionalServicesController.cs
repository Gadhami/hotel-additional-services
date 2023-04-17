using AutoMapper;

using HotelServices.Domain.Entities;
using HotelServices.Domain.Entities.DTO;
using HotelServices.Domain.Interfaces;
using HotelServices.WebAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace HotelServices.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AdditionalServicesController : ControllerBase
{
    private readonly IRepository<AdditionalService> _repository;
    private readonly IMapper _mapper;

    public AdditionalServicesController(IRepository<AdditionalService> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var additionalServices = await _repository.GetAllAsync();
        return Ok(additionalServices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var additionalService = await _repository.GetByIdAsync(id);
        if (additionalService == null)
        {
            return NotFound();
        }

        return Ok(additionalService);
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Create(AdditionalServiceDTO additionalService)
    {
        var service = _mapper.Map<AdditionalService>(additionalService);
        await _repository.CreateAsync(service);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AdditionalService additionalService)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null)
        {
            return NotFound();
        }

        await _repository.UpdateAsync(id, additionalService);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
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