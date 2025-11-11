using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
	private readonly ILogger<BidsController> _logger;
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;

	public BidsController(ILogger<BidsController> logger,
		IMapper mapper,
		IPublishEndpoint publishEndpoint)
	{
		_logger = logger;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
	}

	[HttpPost, Authorize]
	public async Task<IActionResult> PlaceBidAsync(string auctionId, int amount)
	{
		var auction = await DB.Find<Auction>().OneAsync(auctionId);
		if (auction == null)
			return NotFound(); //todo: check with auction service if that has auction

		if (auction.Seller == User.Identity.Name)
			return BadRequest("You cannot place a bid on your own auction");

		var bid = new Bid
		{
			Amount = amount,
			AuctionId = auctionId,
			Bidder = User.Identity.Name
		};

		if (auction.AuctionEnd < DateTime.UtcNow)
			bid.BidStatus = BidStatus.Finished;
		else
		{
			var highBid = await DB.Find<Bid>()
			.Match(a => a.AuctionId == auctionId)
			.Sort(b => b.Descending(x => x.Amount))
			.ExecuteFirstAsync();

			if (highBid != null && amount > highBid.Amount || highBid == null)
			{
				bid.BidStatus = amount > auction.ReservePrice
					? BidStatus.Accepted
					: BidStatus.AcceptedBelowReserve;
			}

			if (highBid != null && bid.Amount <= highBid.Amount)
				bid.BidStatus = BidStatus.TooLow;
		}
		
		await DB.SaveAsync(bid);		
		await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

		return Ok(_mapper.Map<BidDto>(bid));
	}

	[HttpGet("{auctionId}")]
	public async Task<IActionResult> GetBidsForAuctionAsync(string auctionId)
	{
		var bids = await DB.Find<Bid>()
			.Match(a => a.AuctionId == auctionId)
			.Sort(b => b.Descending(a => a.BidTime))
			.ExecuteAsync();

		if (bids.Count == 0)
			return NotFound();

		return Ok(bids.Select(_mapper.Map<BidDto>).ToList());
	}

}
