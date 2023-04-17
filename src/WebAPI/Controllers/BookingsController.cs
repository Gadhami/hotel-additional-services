using AutoMapper;

using HotelServices.Domain.Entities;
using HotelServices.Domain.Entities.DTO;
using HotelServices.Domain.Interfaces;
using HotelServices.WebAPI.Filters;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

namespace HotelServices.WebAPI.Controllers;

[ApiController]
[Route("services/{serviceId}/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IRepository<AdditionalService> _servicesRepository;
    private readonly IMapper _mapper;

    public BookingsController(IRepository<AdditionalService> servicesRepository, IMapper mapper)
    {
        _servicesRepository = servicesRepository;
        _mapper = mapper;
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

        return Ok(service.Bookings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string serviceId, string id)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(serviceId))
        {
            return BadRequest("Incorrect ID");
        }

        var service = await _servicesRepository.GetByIdAsync(serviceId);
        if (service == null)
        {
            return NotFound();
        }

        var booking = service.Bookings.FirstOrDefault(b => b.Id == id);
        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Create([FromRoute] string serviceId, BookingDTO booking)
    {
        if (string.IsNullOrWhiteSpace(serviceId))
        {
            return BadRequest("Incorrect ID");
        }

        var additionalService = await _servicesRepository.GetByIdAsync(serviceId);
        if (additionalService == null)
        {
            return NotFound();
        }

        // TODO: Make sure booking time *is* available!

        var dbBooking = _mapper.Map<Booking>(booking);

        additionalService.Bookings.Add(dbBooking);
        await _servicesRepository.UpdateAsync(serviceId, additionalService);

        // Retrieve the newly created booking from the repository to get its _id property
        var createdBooking = additionalService.Bookings.Last();

        return CreatedAtAction(nameof(GetById), new { serviceId = serviceId, id = createdBooking.Id }, createdBooking);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Booking booking)
    {
        if (string.IsNullOrWhiteSpace(id) || booking.Id != id)
        {
            return BadRequest("Incorrect ID");
        }

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
        existingBooking.End = booking.End;

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

        var booking = existingService.Bookings.FirstOrDefault(b => b.Id == id);
        if (booking == null)
        {
            return NotFound();
        }

        existingService.Bookings.Remove(booking);
        await _servicesRepository.UpdateAsync(existingService.Id, existingService);

        return NoContent();
    }
}