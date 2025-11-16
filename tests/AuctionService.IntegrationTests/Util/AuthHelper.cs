using System.Security.Claims;

namespace AuctionService.IntegrationTests.Util;
internal class AuthHelper
{
	internal static Dictionary<string, object> GetBearerForUser(string username)
	{
		return new Dictionary<string, object> { { ClaimTypes.Name, username } };
	}
}
