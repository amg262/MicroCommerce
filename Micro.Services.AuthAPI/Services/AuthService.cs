using Micro.Services.AuthAPI.Data;
using Micro.Services.AuthAPI.Models;
using Micro.Services.AuthAPI.Models.Dto;
using Micro.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Identity;

namespace Micro.Services.AuthAPI.Services;

/// <summary>
/// Service responsible for handling authentication-related tasks such as user registration, login, and role assignment.
/// </summary>
public class AuthService : IAuthService
{
	private readonly AppDbContext _db;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly IJwtTokenGenerator _jwtTokenGenerator;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthService"/> class.
	/// </summary>
	/// <param name="db">Database context for the application.</param>
	/// <param name="userManager">User manager for handling user-related operations.</param>
	/// <param name="roleManager">Role manager for handling role-related operations.</param>
	/// <param name="jwtTokenGenerator">JWT token generator for generating authentication tokens.</param>
	public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
		IJwtTokenGenerator jwtTokenGenerator)
	{
		_db = db;
		_userManager = userManager;
		_roleManager = roleManager;
		_jwtTokenGenerator = jwtTokenGenerator;
	}

	/// <summary>
	/// Registers a new user with the given registration details.
	/// </summary>
	/// <param name="registrationRequestDto">Data transfer object containing registration details.</param>
	/// <returns>A task that represents the asynchronous operation, returning an error message if registration fails.</returns>
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

	/// <summary>
	/// Authenticates a user and generates a JWT token for valid credentials.
	/// </summary>
	/// <param name="loginRequestDto">Data transfer object containing login details.</param>
	/// <returns>A task that represents the asynchronous operation, returning a LoginResponseDto with user details and token.</returns>
	public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
	{
		var user = _db.ApplicationUsers.FirstOrDefault(u => string.Equals(u.UserName.ToUpper(),
			loginRequestDto.Username.ToUpper()));

		bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

		if (user == null || isValid == false)
		{
			return new LoginResponseDto() {User = null, Token = ""};
		}

		var roles = await _userManager.GetRolesAsync(user);
		var token = _jwtTokenGenerator.GenerateToken(user, roles);

		UserDto userDto = new()
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber,
		};

		LoginResponseDto loginResponseDto = new()
		{
			User = userDto,
			Token = token
		};

		return loginResponseDto;
	}

	/// <summary>
	/// Assigns a role to a user.
	/// </summary>
	/// <param name="email">Email of the user to whom the role will be assigned.</param>
	/// <param name="roleName">The name of the role to assign to the user.</param>
	/// <returns>A task that represents the asynchronous operation, returning true if the role assignment is successful.</returns>
	public async Task<bool> AssignRole(string email, string roleName)
	{
		var user = _db.ApplicationUsers.FirstOrDefault(u => string.Equals(u.Email.ToUpper(), email.ToUpper()));

		if (user != null)
		{
			if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
			{
				_roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
			}

			await _userManager.AddToRoleAsync(user, roleName);
			return true;
		}

		return false;
	}
}