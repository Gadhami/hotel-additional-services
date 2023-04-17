using HotelServices.Application.Common.Mappings;

using AutoMapper;

using FluentAssertions;

using HotelServices.Domain.Entities;
using HotelServices.Domain.Entities.DTO;
using HotelServices.Domain.Interfaces;
using HotelServices.WebAPI.Controllers;
using HotelServices.WebAPI.IntegrationTests.Services;

using Microsoft.AspNetCore.Mvc;

using Moq;

namespace HotelServices.WebAPI.IntegrationTests.Controllers;

public class AdditionalServicesControllerTests
{
    private AdditionalServicesController _controller;
    private IRepository<AdditionalService> _repository;
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public AdditionalServicesControllerTests()
    {
        _configuration = new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>());

        _mapper = _configuration.CreateMapper();
    }

    [SetUp]
    public void Setup()
    {
        // Mock the repository
        _repository = new InMemoryRepository<AdditionalService>();
        _repository.CreateAsync(new AdditionalService { Id = "1", Name = "Service 1" }).Wait();
        _repository.CreateAsync(new AdditionalService { Id = "2", Name = "Service 2" }).Wait();
        _repository.CreateAsync(new AdditionalService { Id = "3", Name = "Service 3" }).Wait();

        _controller = new AdditionalServicesController(_repository, _mapper);
    }

    [Test]
    public async Task GetAll_ReturnsAllServices()
    {
        // Arrange

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new List<AdditionalService>
            {
                new AdditionalService { Id = "1", Name = "Service 1" },
                new AdditionalService { Id = "2", Name = "Service 2" },
                new AdditionalService { Id = "3", Name = "Service 3" }
            });
    }

    [Test]
    public async Task GetById_WithValidId_ReturnsService()
    {
        // Arrange
        var id     = "2";

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new AdditionalService { Id = "2", Name = "Service 2" });
    }

    [Test]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var id     = "99";

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Create_WithValidService_ReturnsCreatedService()
    {
        // Arrange
        var newService = new AdditionalServiceDTO { Name = "Service 4" };

        // Act
        var result     = await _controller.Create(newService);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var outcome    = result as CreatedAtActionResult;
        outcome.Should().NotBeNull();
        (outcome.Value as AdditionalService).Name.Should().BeEquivalentTo(newService.Name);

        //(await _repository.GetByIdAsync(newService.Id)).Should().BeEquivalentTo(newService);
    }

    [Test]
    public async Task Update_WithValidIdAndService_ReturnsNoContent()
    {
        // Arrange
        var id = "2";
        var updatedService = new AdditionalService { Id = "2", Name = "Updated Service 2" };

        // Act
        var result = await _controller.Update(id, updatedService);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        (await _repository.GetByIdAsync(id)).Should().BeEquivalentTo(updatedService);
    }

    [Test]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var mockRepository  = new Mock<IRepository<AdditionalService>>();
        var controller      = new AdditionalServicesController(mockRepository.Object, _mapper);
        var invalidId       = "999";
        var serviceToUpdate = new AdditionalService
        {
            Id          = invalidId,
            Name        = "Updated service",
            Description = "Updated description",
            Price       = 99.99M
        };

        mockRepository.Setup(x => x.GetByIdAsync(invalidId))
                      .ReturnsAsync(null as AdditionalService);

        // Act
        var result = await controller.Update(invalidId, serviceToUpdate);

        // Assert
        result.Should()
              .BeOfType<NotFoundResult>();
        mockRepository.Verify(x => x.GetByIdAsync(invalidId), Times.Once);
        mockRepository.Verify(x => x.UpdateAsync(invalidId, serviceToUpdate), Times.Never);
    }

    [Test]
    public async Task Delete_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var additionalService = new AdditionalService
        {
            Id    = "5",
            Name  = "Test Additional Service",
            Price = 99.99M
        };
        await _repository.CreateAsync(additionalService);

        // Act
        var result = await _controller.Delete(additionalService.Id);

        // Assert
        result.Should()
              .BeOfType<NoContentResult>();

        var services = await _repository.GetAllAsync();
        services.Should()
                .HaveCount(3);
    }

    [Test]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var additionalService = new AdditionalService
        {
            Id    = "1",
            Name  = "Test Additional Service",
            Price = 99.99M
        };
        await _repository.CreateAsync(additionalService);

        // Act
        var result = await _controller.Delete("999");

        // Assert
        result.Should()
              .BeOfType<NotFoundResult>();

        var services = await _repository.GetAllAsync();

        services.Should()
                .Contain(additionalService);
    }
}