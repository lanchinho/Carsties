using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;
internal static class ServiceCollectionExtensions
{
	internal static void RemoveDbContext<T>(this IServiceCollection services)
	{
		var descriptor = services.SingleOrDefault(d =>
			d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

		if (descriptor != null) services.Remove(descriptor);
	}


	internal static void EnsureCreated<T>(this IServiceCollection services)
	{
		var sp = services.BuildServiceProvider();
		using var scope = sp.CreateScope();
		var scoppedServices = scope.ServiceProvider;
		var db = scoppedServices.GetRequiredService<AuctionDbContext>();

		db.Database.Migrate();
		DbHelper.InitDbForTests(db);
	}
}
