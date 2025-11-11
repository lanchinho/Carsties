using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
	private readonly AuctionDbContext _auctionContext;

	public AuctionFinishedConsumer(AuctionDbContext auctionContext)
	{
		_auctionContext = auctionContext;
	}	

	public async Task Consume(ConsumeContext<AuctionFinished> context)
	{
		var auction = await _auctionContext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId));
		if (context.Message.ItemSold)
		{
			auction.Winner = context.Message.Winner;
			auction.SoldAmount = context.Message.Amount;
		}

		auction.Status = auction.SoldAmount > auction.ReservePrice
			? Status.Finished : Status.ReserveNotMet;

		await _auctionContext.SaveChangesAsync();		
	}
}
