using Micro.Services.AuthAPI.Models;

namespace Micro.Services.AuthAPI.Services.IService;

public interface IJwtTokenGenerator
{
	string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
}