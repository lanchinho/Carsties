using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
	private readonly AuctionDbContext _auctionDbContext;

	public BidPlacedConsumer(AuctionDbContext auctionDbContext)
	{
		_auctionDbContext = auctionDbContext;
	}

	public async Task Consume(ConsumeContext<BidPlaced> context)
	{
		var auctionId = Guid.Parse(context.Message.AuctionId);
		var auction = await _auctionDbContext.Auctions.FindAsync(auctionId);

		if(auction.CurrentHighBid == null 
			|| context.Message.BidStatus.Contains("Accepted")
			&& context.Message.Amount > auction.CurrentHighBid)
		{
			auction.CurrentHighBid = context.Message.Amount;
		}

		await _auctionDbContext.SaveChangesAsync();
	}
}
