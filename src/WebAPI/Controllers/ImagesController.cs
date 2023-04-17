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
    public async Task<IActionResult> GetById([FromRoute] string serviceId, [FromRoute] string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId) || string.IsNullOrWhiteSpace(serviceId))
        {
            return BadRequest("Incorrect ID");
        }

        var service = await _servicesRepository.GetByIdAsync(serviceId);
        if (service == null)
        {
            return NotFound();
        }

        var image = service.Images.FirstOrDefault(img => img.Id == imageId);
        if (image == null)
        {
            return NotFound();
        }

        return Ok(image);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] string serviceId, ImageDTO image)
    {
        var additionalService = await _servicesRepository.GetByIdAsync(serviceId);
        if (additionalService == null)
        {
            return NotFound();
        }

        var result = await UploadImages();
        if (result != null)
        {
            return result!;
        }

        var img = _mapper.Map<Image>(image);

        additionalService.Images.Add(img);
        await _servicesRepository.UpdateAsync(serviceId, additionalService);

        return CreatedAtAction(nameof(GetById), new { serviceId = serviceId, id = img.Id }, img);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string serviceId, [FromRoute] string imageId, Image image)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
        {
            return BadRequest("Incorrect Service ID");
        }

        if (string.IsNullOrWhiteSpace(imageId) || image.Id != imageId)
        {
            return BadRequest("Incorrect ID");
        }

        var existingService = await _servicesRepository.GetByIdAsync(serviceId);
        if (existingService == null)
        {
            return NotFound();
        }

        var existingImage = existingService.Images.FirstOrDefault(b => b.Id == imageId);
        if (existingImage == null)
        {
            return NotFound();
        }

        if (HttpContext.Request.Form?.Files?.Count > 0)
        {
            var result = await UploadImages();
            if (result != null)
            {
                return result!;
            }
        }

        existingImage.Name = image.Name;
        existingImage.URL  = image.URL;

        await _servicesRepository.UpdateAsync(existingService.Id, existingService);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string serviceId, [FromRoute] string imageId)
    {
        if (string.IsNullOrWhiteSpace(serviceId) || string.IsNullOrWhiteSpace(imageId))
        {
            return BadRequest("Incorrect ID");
        }

        var existingService = await _servicesRepository.GetByIdAsync(serviceId);
        if (existingService == null)
        {
            return NotFound();
        }

        var image = existingService.Images.FirstOrDefault(img => img.Id == imageId);
        if (image == null)
        {
            return NotFound();
        }

        existingService.Images.Remove(image);
        await _servicesRepository.UpdateAsync(existingService.Id, existingService);

        // TODO: Delete Image File

        return NoContent();
    }

    private async Task<IActionResult?> UploadImages()
    {
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

            await UploadOneFile(file);
        }

        return null;
    }

    private async Task<bool> UploadOneFile(IFormFile file)
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
}