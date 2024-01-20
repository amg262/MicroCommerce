using Micro.Services.RewardsAPI.Data;
using Micro.Services.RewardsAPI.Message;
using Micro.Services.RewardsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.RewardsAPI.Service;

public class RewardService : IRewardService
{
	private readonly DbContextOptions<AppDbContext> _dbOptions;

	public RewardService(DbContextOptions<AppDbContext> dbOptions)
	{
		_dbOptions = dbOptions;
	}

	public async Task UpdateRewards(RewardsMessage rewardsMessage)
	{
		try
		{
			Rewards rewards = new()
			{
				OrderId = rewardsMessage.OrderId,
				RewardsActivity = rewardsMessage.RewardsActivity,
				UserId = rewardsMessage.UserId,
				RewardsDate = DateTime.Now
			};
			await using var db = new AppDbContext(_dbOptions);
			await db.Rewards.AddAsync(rewards);
			await db.SaveChangesAsync();
		}
		catch (Exception ex)
		{
			throw;
		}
	}
}