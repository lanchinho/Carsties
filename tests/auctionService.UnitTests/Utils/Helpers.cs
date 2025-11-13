using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;
internal class Helpers
{
	public static ClaimsPrincipal GetClaimsPrincipal()
	{
		var claims = new List<Claim> {
			new("username", "unitarius"),
			new(ClaimTypes.Name, "Bob Unitarius")
		};
		var identity = new ClaimsIdentity(claims, "testing");
		return new ClaimsPrincipal(identity);
	}
}
