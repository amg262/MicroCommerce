using Micro.Services.RewardsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.RewardsAPI.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<Rewards> Rewards { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}
}