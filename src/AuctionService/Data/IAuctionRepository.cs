using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
	Task<List<AuctionDTO>> GetAuctionAsync(string date);
	Task<AuctionDTO> GetAuctionByIdAsync(Guid id);
	Task<Auction> GetAuctionEntityById(Guid id);
	void AddAuction(Auction auction);
	void RemoveAuction(Auction auction);
	Task<bool> SaveChangesAsync();
}
