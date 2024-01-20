using Micro.Services.EmailAPI.Message;
using Micro.Services.EmailAPI.Models.Dto;

namespace Micro.Services.EmailAPI.Service;

public interface IEmailService
{
	Task EmailCartAndLog(CartDto cartDto);
	Task RegisterUserEmailAndLog(string email);
	Task LogOrderPlaced(RewardsMessage rewardsDto);
}