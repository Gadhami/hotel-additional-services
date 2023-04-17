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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace HotelServices.WebAPI.IntegrationTests.Controllers;

public class ImagesControllerTests
{
    private ImagesController _controller;
    private IRepository<AdditionalService> _repository;
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _host;

    public ImagesControllerTests(IWebHostEnvironment host)
    {
        _configuration = new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>());

        _mapper = _configuration.CreateMapper();
        _host   = host;
    }

    [SetUp]
    public void Setup()
    {
        _repository = new InMemoryRepository<AdditionalService>();
        _controller = new ImagesController(_repository, _mapper, _host);
            
        // Seed the repository with test data
        var additionalService = new AdditionalService
        {
            Id          = "1",
            Name        = "Test Service",
            Description = "Test Service Description",
            Price       = 9.99m,
            Bookings    = new List<Booking>(),
            Images = new List<Image>
            {
                new Image
                {
                    Id    = "1",
                    Name  = "abc.png",
                    URL   = "http://google.com/abc.png"
                },
                new Image
                {
                    Id    = "2",
                    Name  = "xyz.png",
                    URL   = "http://google.com/xyz.png"
                }
            }
        };

        _repository.CreateAsync(additionalService)
                   .Wait();
    }

    [Test]
    public async Task GetAll_ShouldReturnOkObjectResult()
    {
        // Arrange
        var serviceId     = "1";
        var expectedImage = new Image
        {
            Id   = "1",
            Name = "abc.png",
            URL  = "http://google.com/abc.png"
        };

        // Act
        var result       = await _controller.GetAll(serviceId) as OkObjectResult;
        var actualImages = result.Value as List<Image>;
        var actualImage  = actualImages!.FirstOrDefault(b => b.Id == expectedImage.Id);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);

        actualImage.Should().NotBeNull();
        actualImage!.Id.Should().Be(expectedImage.Id);
        actualImage.Name.Should().Be("abc.png");
        actualImage.URL.Should().Be("http://google.com/abc.png");
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
        var ImageId   = "1";

        // Act
        var result = await _controller.GetById(serviceId, ImageId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var image = (result as OkObjectResult).Value as Image;
        image!.Id.Should().Be(ImageId);
        image.Name.Should().Be("abc.png");
        image.URL.Should().Be("http://google.com/abc.png");
    }

    [Test]
    public async Task GetById_WithIncorrectId_ShouldReturnNotFoundResult()
    {
        // Arrange
        var serviceId = "1";
        var ImageId   = "0";

        // Act
        var result = await _controller.GetById(serviceId, ImageId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetById_WithNonexistentService_ShouldReturnNotFoundResult()
    {
        // Arrange
        var serviceId = "2";
        var ImageId   = "1";

        // Act
        var result = await _controller.GetById(serviceId, ImageId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetById_WithNonexistentImage_ShouldReturnNotFoundResult()
    {
        // Arrange
        var serviceId = "1";
        var ImageId   = "3";

        // Act
        var result    = await _controller.GetById(serviceId, ImageId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Create_ShouldReturnNotFound_WhenServiceNotFound()
    {
        // Arrange
        var newImage = new ImageDTO
        {
            Name = "abc.png",
            URL  = "http://google.com/abc.png"
        };

        // Act
        var result = await _controller.Create("2", newImage);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    //[Test]
    //public async Task Create_ShouldReturnCreatedAtActionResult_WhenValidInput()
    //{
    //    // Arrange
    //    var serviceId = "1";
    //    var newImage  = new ImageDTO
    //    {
    //        Name = "abc.png",
    //        URL  = "http://google.com/abc.png"
    //    };

    //    // Act
    //    var result = await _controller.Create(serviceId, newImage);
    //    var createdAtActionResult = result as CreatedAtActionResult;
    //    var createdImage = createdAtActionResult?.Value as Image;

    //    // Assert
    //    createdAtActionResult.Should().NotBeNull();
    //    createdAtActionResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
    //    createdAtActionResult.ActionName.Should().Be(nameof(ImagesController.GetById));

    //    createdImage.Should().NotBeNull();
    //    createdImage!.Id.Should().NotBe("0");
    //    createdImage.Start.Should().Be(newImage.Start);
    //    createdImage.End.Should().Be(newImage.End);
    //}

    //[Test]
    //public async Task Update_ExistingImage_ShouldUpdateImage()
    //{
    //    // Arrange
    //    var Image        = new Image { Id = Guid.NewGuid().ToString(), Start = DateTime.UtcNow, End = DateTime.UtcNow.AddDays(1) };
    //    var service        = new AdditionalService { Id = Guid.NewGuid().ToString(), Name = "Test Service", Images = new List<Image> { Image } };
    //    await _repository.CreateAsync(service);
    //    var updatedImage = new Image { Id = Image.Id, Start = Image.Start, End = Image.End.AddDays(3) };

    //    // Act
    //    var result         = await _controller.Update(service.Id, Image.Id, updatedImage) as NoContentResult;
    //    var updatedService = await _repository.GetByIdAsync(service.Id);

    //    // Assert
    //    result.Should().NotBeNull();
    //    result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    //    updatedService.Images.Should().ContainSingle(b => b.Id == Image.Id && b.Start == Image.Start && b.End == updatedImage.End);
    //}

    [Test]
    public async Task Delete_ExistingImage_ShouldDeleteImage()
    {
        // Arrange
        var image          = new Image
        {
            Id   = Guid.NewGuid().ToString(),
            Name = "abc.png",
            URL  = "http://google.com/abc.png"
        };
        var service        = new AdditionalService { Id = Guid.NewGuid().ToString(), Name = "Test Service", Images = new List<Image> { image } };
        await _repository.CreateAsync(service);

        // Act
        var result         = await _controller.Delete(service.Id, image.Id) as NoContentResult;
        var updatedService = await _repository.GetByIdAsync(service.Id);

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        updatedService.Images.Should().BeEmpty();
    }
}