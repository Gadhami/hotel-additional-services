using System.Net;

using HotelServices.Application.Common.Mappings;

using AutoMapper;

using FluentAssertions;

using HotelServices.Domain.Entities;
using HotelServices.Domain.Entities.DTO;
using HotelServices.Domain.Interfaces;
using HotelServices.WebAPI.Controllers;
using HotelServices.WebAPI.IntegrationTests.Services;

using Microsoft.AspNetCore.Mvc;

namespace HotelServices.WebAPI.IntegrationTests.Controllers;

public class BookingsControllerTests
{
    private BookingsController _controller;
    private IRepository<AdditionalService> _repository;
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public BookingsControllerTests()
    {
        _configuration = new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>());

        _mapper = _configuration.CreateMapper();
    }

    [SetUp]
    public void Setup()
    {
        _repository = new InMemoryRepository<AdditionalService>();
        _controller = new BookingsController(_repository, _mapper);

        // Seed the repository with test data
        var additionalService = new AdditionalService
        {
            Id          = "1",
            Name        = "Test Service",
            Description = "Test Service Description",
            Price       = 9.99m,
            Bookings    = new List<Booking>
            {
                new Booking
                {
                    Id    = "1",
                    Start = DateTime.UtcNow.AddDays(1),
                    End   = DateTime.UtcNow.AddDays(2)
                },
                new Booking
                {
                    Id    = "2",
                    Start = DateTime.UtcNow.AddDays(2),
                    End   = DateTime.UtcNow.AddDays(3)
                }            
            },
            Images = new List<Image>()
        };

        _repository.CreateAsync(additionalService)
                   .Wait();
    }

    [Test]
    public async Task GetAll_ShouldReturnOkObjectResult()
    {
        // Arrange
        var serviceId       = "1";
        var expectedBooking = new Booking
        {
            Id    = "1",
            Start = DateTime.UtcNow.AddDays(1),
            End   = DateTime.UtcNow.AddDays(2)
        };

        // Act
        var result         = await _controller.GetAll(serviceId) as OkObjectResult;
        var actualBookings = result.Value as List<Booking>;
        var actualBooking  = actualBookings!.FirstOrDefault(b => b.Id == expectedBooking.Id);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);

        actualBooking.Should().NotBeNull();
        actualBooking!.Id.Should().Be(expectedBooking.Id);
        actualBooking.Start.Should()
                           .BeCloseTo(expectedBooking.Start, new TimeSpan(0, 0, 30));
        actualBooking.End.Should()
                         .BeCloseTo(expectedBooking.End, new TimeSpan(0, 0, 30));
    }

    [Test]
    public async Task GetAll_WithIncorrectServiceId_ShouldReturnNotFoundObjectResult()
    {
        // Arrange
        var serviceId = "0";

        // Act
        var result = await _controller.GetAll(serviceId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetById_ShouldReturnOkObjectResult()
    {
        // Arrange
        var serviceId = "1";
        var bookingId = "1";

        // Act
        var result = await _controller.GetById(serviceId, bookingId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var booking = (result as OkObjectResult).Value as Booking;
        booking!.Id.Should().Be(bookingId);
        booking.Start.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), new TimeSpan(0, 0, 30));
        booking.End.Should().BeCloseTo(DateTime.UtcNow.AddDays(2), new TimeSpan(0, 0, 30));
    }

    [Test]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var serviceId = "1";
        var bookingId = "0";

        // Act
        var result = await _controller.GetById(serviceId, bookingId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetById_WithNonexistentService_ShouldReturnNotFoundResult()
    {
        // Arrange
        var serviceId = "2";
        var bookingId = "1";

        // Act
        var result = await _controller.GetById(serviceId, bookingId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetById_WithNonexistentBooking_ShouldReturnNotFoundResult()
    {
        // Arrange
        var serviceId = "1";
        var bookingId = "3";

        // Act
        var result = await _controller.GetById(serviceId, bookingId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Create_ShouldReturnNotFound_WhenServiceNotFound()
    {
        // Arrange
        var newBooking = new BookingDTO
        {
            Start = DateTime.Now.AddDays(1),
            End   = DateTime.Now.AddDays(2)
        };

        // Act
        var result = await _controller.Create("2", newBooking);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Create_ShouldReturnCreatedAtActionResult_WhenValidInput()
    {
        // Arrange
        var serviceId  = "1";
        var newBooking = new BookingDTO
        {
            Start = DateTime.Now.AddDays(1),
            End   = DateTime.Now.AddDays(2)
        };

        // Act
        var result = await _controller.Create(serviceId, newBooking);
        var createdAtActionResult = result as CreatedAtActionResult;
        var createdBooking = createdAtActionResult?.Value as Booking;

        // Assert
        createdAtActionResult.Should().NotBeNull();
        createdAtActionResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
        createdAtActionResult.ActionName.Should().Be(nameof(BookingsController.GetById));

        createdBooking.Should().NotBeNull();
        createdBooking!.Id.Should().NotBe("0");
        createdBooking.Start.Should().Be(newBooking.Start);
        createdBooking.End.Should().Be(newBooking.End);
    }
}