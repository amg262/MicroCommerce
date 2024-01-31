using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

/// <summary>
/// Provides authentication services such as user registration, login, and role assignment.
/// </summary>
public class AuthService : IAuthService
{
	private readonly IBaseService _baseService;

	/// <summary>
	/// Initializes a new instance of the AuthService class.
	/// </summary>
	/// <param name="baseService">The base service used for sending HTTP requests.</param>
	public AuthService(IBaseService baseService)
	{
		_baseService = baseService;
	}

	/// <summary>
	/// Registers a new user asynchronously.
	/// </summary>
	/// <param name="registrationRequestDto">The registration request containing user details.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO.</returns>
	public async Task<ResponseDto?> RegisterUserAsync(RegistrationRequestDto registrationRequestDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = registrationRequestDto,
			Url = SD.AuthAPIBase + "/api/auth/register",
		});
	}

	/// <summary>
	/// Logs in a user asynchronously.
	/// </summary>
	/// <param name="loginRequestDto">The login request containing user credentials.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO.</returns>
	public async Task<ResponseDto?> LoginUserAsync(LoginRequestDto loginRequestDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = loginRequestDto,
			Url = SD.AuthAPIBase + "/api/auth/login",
		}, withBearer: false);
	}

	/// <summary>
	/// Assigns a role to a user asynchronously.
	/// </summary>
	/// <param name="registrationRequestDto">The registration request containing user details and the role to be assigned.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO.</returns>
	public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registrationRequestDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = registrationRequestDto,
			Url = SD.AuthAPIBase + "/api/auth/AssignRole",
		}, withBearer: false);
	}
}