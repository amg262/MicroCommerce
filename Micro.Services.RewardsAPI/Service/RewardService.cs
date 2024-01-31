using Micro.Services.RewardsAPI.Data;
using Micro.Services.RewardsAPI.Message;
using Micro.Services.RewardsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.RewardsAPI.Service;

/// <summary>
/// Service class to manage reward operations.
/// </summary>
public class RewardService : IRewardService
{
	private readonly DbContextOptions<AppDbContext> _dbOptions;

	/// <summary>
	/// Initializes a new instance of the <see cref="RewardService"/> class.
	/// </summary>
	/// <param name="dbOptions">Configuration options for the database context.</param>
	public RewardService(DbContextOptions<AppDbContext> dbOptions)
	{
		_dbOptions = dbOptions;
	}

	/// <summary>
	/// Updates the rewards for a user based on the provided rewards message.
	/// </summary>
	/// <param name="rewardsMessage">Message containing details about the reward activity.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task UpdateRewards(RewardsMessage rewardsMessage)
	{
		try
		{
			// Create a new rewards object based on the rewards message and add it to the database.
			Rewards rewards = new()
			{
				OrderId = rewardsMessage.OrderId,
				RewardsActivity = rewardsMessage.RewardsActivity,
				UserId = rewardsMessage.UserId,
				RewardsDate = DateTime.Now
			};
			// Using the database context to add and save the new rewards object
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