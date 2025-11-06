using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
	private readonly AuctionDbContext _context;
	private readonly ILogger<AuctionsController> _logger;
	private readonly IMapper _mapper;

	public AuctionsController(
		AuctionDbContext context,
		ILogger<AuctionsController> logger,
		IMapper mapper)
	{
		_context = context;
		_logger = logger;
		_mapper = mapper;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllAuctions(string date)
	{
		var query = _context.Auctions
				.AsNoTracking()
				.OrderBy(x => x.Item.Make)
				.AsQueryable();

		if (!string.IsNullOrWhiteSpace(date))
			query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);

		return Ok(await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync());
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetAuctionsById(Guid id)
	{
		var auction = await _context
			.Auctions
			.AsNoTracking()
			.Include(x => x.Item)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (auction == null)
			return NotFound();

		return Ok(_mapper.Map<AuctionDTO>(auction));
	}

	[HttpPost]
	public async Task<IActionResult> CreateAuction(CreateAuctionDto auctionDto)
	{
		var auction = _mapper.Map<Auction>(auctionDto);
		auction.Seller = "test";

		await _context.Auctions.AddAsync(auction);
		var result = await _context.SaveChangesAsync() > 0;
		if (!result) return BadRequest("Could not save changes to the DB");


		return CreatedAtAction(nameof(GetAuctionsById),
			new { auction.Id }, _mapper.Map<AuctionDTO>(auction));
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		var auction = await _context.Auctions
			.Include(x => x.Item)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (auction == null) return NotFound();

		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

		var result = await _context.SaveChangesAsync() > 0;
		if (result) return Ok();

		return BadRequest("Problem saving changes");
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAuction(Guid id)
	{
		var auction = await _context.Auctions.FindAsync(id);
		if (auction == null) return NotFound();

		_context.Auctions.Remove(auction);
		var result = await _context.SaveChangesAsync() > 0;

		if (!result) return BadRequest("Could not delete auction");

		return Ok();
	}

}
