using AutoMapper;

using HotelServices.Domain.Entities;
using HotelServices.Domain.Entities.DTO;
using HotelServices.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace HotelServices.WebAPI.Controllers;

[ApiController]
[Route("services/{serviceId:int}/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IRepository<AdditionalService> _servicesRepository;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _host;

    public ImagesController(IRepository<AdditionalService> servicesRepository, IMapper mapper, IWebHostEnvironment host)
    {
        _servicesRepository = servicesRepository;
        _mapper             = mapper;
        _host               = host;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] string serviceId)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Incorrect ID");
        }

        var Image = await _servicesRepository.GetByIdAsync(id);
        if (Image == null)
        {
            return NotFound();
        }

        return Ok(Image);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] string serviceId, ImageDTO image)
    {
        var additionalService = await _servicesRepository.GetByIdAsync(serviceId);
        if (additionalService == null)
        {
            return NotFound();
        }

        var files = HttpContext.Request.Form?.Files;
        if (files == null || files.Count == 0)
        {
            return BadRequest("File not selected.");
        }

        foreach (var file in files)
        {
            // Check if the file exists
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not selected.");
            }

            // Check the file type
            if (!file.ContentType.Contains("image"))
            {
                return BadRequest("File is not an image.");
            }

            await UploadImage(file);
        }

        var img = _mapper.Map<Image>(image);

        additionalService.Images.Add(img);
        await _servicesRepository.UpdateAsync(serviceId, additionalService);

        return CreatedAtAction(nameof(GetById), new { serviceId = serviceId, id = img.Id }, img);
    }

    private async Task<bool> UploadImage(IFormFile file)
    {
        // Create a unique filename
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        // Create a directory to save the file
        string uploadPath = Path.Combine(_host.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadPath);

        // Save the file to the directory
        string filePath = Path.Combine(uploadPath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the file path
        return true;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Image Image)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Incorrect ID");
        }

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Incorrect ID");
        }

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