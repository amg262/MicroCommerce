using Micro.Web.Models;

namespace Micro.Web.Service.IService;

public interface IAuthService
{
	Task<ResponseDto?> RegisterUserAsync(RegistrationRequestDto registrationRequestDto);
	Task<ResponseDto?> LoginUserAsync(LoginRequestDto loginRequestDto);
	Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto userDto);
	
}