using HotelServices.Domain.Entities;
using HotelServices.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace HotelServices.WebAPI.Controllers;

[ApiController]
[Route("services/{serviceId:int}/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IRepository<AdditionalService> _servicesRepository;

    public ImagesController(IRepository<AdditionalService> servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] int serviceId)
    {
        if (serviceId <= 0)
        {
            return BadRequest("Incorrect Service ID!");
        }

        var service = await _servicesRepository.GetByIdAsync(serviceId);

        if (service == null)
        {
            return NotFound("Service not found!");
        }

        return Ok(service.Images);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var Image = await _servicesRepository.GetByIdAsync(id);
        if (Image == null)
        {
            return NotFound();
        }

        return Ok(Image);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int serviceId, Image Image)
    {
        var additionalService = await _servicesRepository.GetByIdAsync(serviceId);
        if (additionalService == null)
        {
            return NotFound();
        }

        // TODO: Upload Image

        additionalService.Images.Add(Image);
        await _servicesRepository.UpdateAsync(serviceId, additionalService);

        return CreatedAtAction(nameof(GetById), new { serviceId = serviceId, id = Image.Id }, Image);

    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Image Image)
    {
        var existingService = await _servicesRepository.GetByIdAsync(id);
        if (existingService == null)
        {
            return NotFound();
        }

        var existingImage = existingService.Images.FirstOrDefault(b => b.Id == id);
        if (existingImage == null)
        {
            return NotFound();
        }

        // TODO: Upload Image

        await _servicesRepository.UpdateAsync(existingService.Id, existingService);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingService = await _servicesRepository.GetByIdAsync(id);
        if (existingService == null)
        {
            return NotFound();
        }

        await _servicesRepository.DeleteAsync(id);

        // TODO: Delete Image File

        return NoContent();
    }
}