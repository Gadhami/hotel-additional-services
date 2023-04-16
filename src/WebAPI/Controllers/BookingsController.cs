using HotelServices.Domain.Entities;
using HotelServices.Domain.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace HotelServices.WebAPI.Controllers;

[ApiController]
[Route("services/{serviceId:int}/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IRepository<AdditionalService> _servicesRepository;

    public BookingsController(IRepository<AdditionalService> servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] int serviceId)
    {
        if (serviceId <=  0)
        {
            return BadRequest("Incorrect Service ID!");
        }

        var service = await _servicesRepository.GetByIdAsync(serviceId);

        if (service == null)
        {
            return NotFound("Service not found!");
        }

        return Ok(service.Bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _servicesRepository.GetByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int serviceId, Booking booking)
    {
        var additionalService = await _servicesRepository.GetByIdAsync(serviceId);
        if (additionalService == null)
        {
            return NotFound();
        }

        // TODO: Make sure booking time *is* available!

        additionalService.Bookings.Add(booking);
        await _servicesRepository.UpdateAsync(serviceId, additionalService);

        return CreatedAtAction(nameof(GetById), new { serviceId = serviceId, id = booking.Id }, booking);

    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Booking booking)
    {
        var existingService = await _servicesRepository.GetByIdAsync(id);
        if (existingService == null)
        {
            return NotFound();
        }

        var existingBooking = existingService.Bookings.FirstOrDefault(b => b.Id == id);
        if (existingBooking == null)
        {
            return NotFound();
        }

        // TODO: Make sure booking time *is* available!

        existingBooking.Start = booking.Start;
        existingBooking.End   = booking.End;

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

        return NoContent();
    }
}