using Micro.Services.AuthAPI.Data;
using Micro.Services.AuthAPI.Models;
using Micro.Services.AuthAPI.Models.Dto;
using Micro.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Identity;

namespace Micro.Services.AuthAPI.Services;

public class AuthService : IAuthService
{
	private readonly AppDbContext _db;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;

	public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
	{
		_db = db;
		_userManager = userManager;
		_roleManager = roleManager;
	}

	public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
	{
		ApplicationUser user = new()
		{
			Email = registrationRequestDto.Email,
			UserName = registrationRequestDto.Email,
			NormalizedEmail = registrationRequestDto.Email.ToUpper(),
			Name = registrationRequestDto.Name,
			PhoneNumber = registrationRequestDto.PhoneNumber,
		};

		try
		{
			var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);

			if (result.Succeeded)
			{
				var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

				UserDto userDto = new()
				{
					Id = userToReturn.Id,
					Name = userToReturn.Name,
					Email = userToReturn.Email,
					PhoneNumber = userToReturn.PhoneNumber,
				};

				return string.Empty;
			}
			else
			{
				return result.Errors.FirstOrDefault().Description;
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		return "Error Occured";
	}

	public Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
	{
		throw new NotImplementedException();
	}
}