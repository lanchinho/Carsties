using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository : IAuctionRepository
{
	private readonly AuctionDbContext _context;
	private readonly IMapper _mapper;

	public AuctionRepository(AuctionDbContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public void AddAuction(Auction auction)
	{
		_context.Auctions.Add(auction);
	}

	public async Task<List<AuctionDTO>> GetAuctionAsync(string date)
	{
		var query = _context.Auctions
			.AsNoTracking()
			.OrderBy(x => x.Item.Make)
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(date))
			query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);

		return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
	}

	public async Task<AuctionDTO> GetAuctionByIdAsync(Guid id)
	{
		return await _context.Auctions
			.AsNoTracking()
			.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(x => x.Id == id);
	}

	public async Task<Auction> GetAuctionEntityById(Guid id)
	{
		return await _context.Auctions
			.Include(x => x.Item)
			.FirstOrDefaultAsync();
	}

	public void RemoveAuction(Auction auction)
	{
		_context.Auctions.Remove(auction);
	}

	public async Task<bool> SaveChangesAsync()
	{
		return await _context.SaveChangesAsync() > 0;
	}
}
