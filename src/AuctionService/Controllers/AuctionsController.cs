using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
	private readonly IAuctionRepository _auctionRepository;
	private readonly ILogger<AuctionsController> _logger;
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;

	public AuctionsController(
		IAuctionRepository auctionRepository,
		ILogger<AuctionsController> logger,
		IMapper mapper,
		IPublishEndpoint publishEndpoint)
	{
		_auctionRepository = auctionRepository;
		_logger = logger;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllAuctions(string date)
	{
		return Ok(await _auctionRepository.GetAuctionAsync(date));
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetAuctionsById(Guid id)
	{
		var auction = await _auctionRepository.GetAuctionByIdAsync(id);

		if (auction == null)
			return NotFound();

		return Ok(auction);
	}

	[HttpPost, Authorize]
	public async Task<IActionResult> CreateAuction(CreateAuctionDto auctionDto)
	{
		var auction = _mapper.Map<Auction>(auctionDto);
		auction.Seller = User.Identity.Name;

		_logger.LogTrace("Try to: save new auction to DB and publish message to the service bus: {auction}", auction);
		 _auctionRepository.AddAuction(auction);		
		var newAuction = _mapper.Map<AuctionDTO>(auction);
		await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));	

		var result = await _auctionRepository.SaveChangesAsync();
		if (!result) return BadRequest("Could not save changes to the DB");

		return CreatedAtAction(nameof(GetAuctionsById),
			new { auction.Id }, newAuction);
	}

	[HttpPut("{id}"), Authorize]
	public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		var auction = await _auctionRepository.GetAuctionEntityById(id);

		if (auction == null) return NotFound();
		if (!User.Identity.Name.Equals(auction.Seller)) return Forbid();

		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

		await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

		var result = await _auctionRepository.SaveChangesAsync();
		if (result) return Ok();

		return BadRequest("Problem saving changes");
	}

	[HttpDelete("{id}"), Authorize]
	public async Task<IActionResult> DeleteAuction(Guid id)
	{
		var auction = await _auctionRepository.GetAuctionEntityById(id);
		if (auction == null) return NotFound();

		if (!User.Identity.Name.Equals(auction.Seller)) return Forbid();

		_auctionRepository.RemoveAuction(auction);
		await _publishEndpoint.Publish<AuctionDeleted>(new AuctionDeleted { Id = auction.Id.ToString() });
		var result = await _auctionRepository.SaveChangesAsync();

		if (!result) return BadRequest("Could not delete auction");

		return Ok();
	}

}
