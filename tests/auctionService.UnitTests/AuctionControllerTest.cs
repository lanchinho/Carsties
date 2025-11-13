using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuctionService.UnitTests;
public class AuctionControllerTest
{
	private readonly Mock<IAuctionRepository> _auctionRepo;
	private readonly Mock<IPublishEndpoint> _publishEndpoint;
	private readonly Mock<ILogger<AuctionsController>> _loggerMock;
	private readonly Fixture _fixture;
	private readonly AuctionsController _controller;
	private readonly IMapper _mapper;

	public AuctionControllerTest()
	{
		_fixture = new Fixture();
		_auctionRepo = new Mock<IAuctionRepository>();
		_loggerMock = new Mock<ILogger<AuctionsController>>();
		_publishEndpoint = new Mock<IPublishEndpoint>();

		var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Debug);
		});

		var mockMapper = new MapperConfiguration(mc =>
		{
			mc.AddMaps(typeof(MappingProfiles).Assembly);
		}, loggerFactory).CreateMapper().ConfigurationProvider;
		_mapper = new Mapper(mockMapper);

		_controller = new AuctionsController(_auctionRepo.Object, _loggerMock.Object, _mapper, _publishEndpoint.Object)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
			}
		};		
	}

	[Fact]
	public async Task GetAuctions_WithNoParams_Returns10Auctions()
	{
		//Arrange
		var auctions = _fixture.CreateMany<AuctionDTO>(10).ToList();
		_auctionRepo.Setup(repo => repo.GetAuctionAsync(null)).ReturnsAsync(auctions);

		//Act
		var result = await _controller.GetAllAuctions(null) as OkObjectResult;	

		//Assert
		Assert.NotNull(result);
		Assert.Equal(200, result.StatusCode);

		var returnedAuctions = Assert.IsType<List<AuctionDTO>>(result.Value);
		Assert.Equal(10, returnedAuctions.Count);
	}

	[Fact]
	public async Task GetAuctionsById_WithValidGuid_ReturnsAuction()
	{
		//Arrange
		var auction = _fixture.Create<AuctionDTO>();
		_auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

		//Act
		var result = await _controller.GetAuctionsById(auction.Id) as OkObjectResult;

		//Assert
		Assert.NotNull(result);
		Assert.Equal(200, result.StatusCode);

		var returnedAuction = Assert.IsType<AuctionDTO>(result.Value);
		Assert.Equal(auction.Make, returnedAuction.Make);
		Assert.Equal(auction.Model, returnedAuction.Model);
		Assert.Equal(auction.Seller, returnedAuction.Seller);
	}

	[Fact]
	public async Task GetAuctionsById_WithInvalidGuid_ReturnsNotFound()
	{
		//Arrange		
		_auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

		//Act
		var result = await _controller.GetAuctionsById(Guid.NewGuid()) as NotFoundResult;

		//Assert
		Assert.NotNull(result);
		Assert.Equal(404, result.StatusCode);
	}

	[Fact]
	public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
	{
		//Arrange	
		var auction = _fixture.Create<CreateAuctionDto>();
		_auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

		//Act
		var result = await _controller.CreateAuction(auction) as CreatedAtActionResult;

		//Assert
		Assert.NotNull(result);
		Assert.Equal("GetAuctionsById", result.ActionName);
		Assert.Equal(201, result.StatusCode);
	}

	[Fact]
	public async Task CreateAuction_FailedSave_Returns400BadRequest()
	{
		//Arrange	
		var auction = _fixture.Create<CreateAuctionDto>();
		_auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
		_auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

		//Act
		var result = await _controller.CreateAuction(auction) as BadRequestObjectResult;

		//Assert
		Assert.NotNull(result);
		Assert.Equal(400, result.StatusCode);
		Assert.Equal("Could not save changes to the DB", result.Value);
	}

	[Fact]
	public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
	{
		throw new NotImplementedException();
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
	{
		throw new NotImplementedException();
	}

	[Fact]
	public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
	{
		throw new NotImplementedException();
	}

	[Fact]
	public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
	{
		throw new NotImplementedException();
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
	{
		throw new NotImplementedException();
	}

	[Fact]
	public async Task DeleteAuction_WithInvalidUser_Returns403Response()
	{
		throw new NotImplementedException();
	}
}
