using Micro.Services.RewardsAPI.Message;

namespace Micro.Services.RewardsAPI.Service;

public interface IRewardService
{
	Task UpdateRewards(RewardsMessage rewardsMessage);
}