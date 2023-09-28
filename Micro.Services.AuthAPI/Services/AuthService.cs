﻿using Micro.Services.AuthAPI.Data;
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
	private readonly IJwtTokenGenerator _jwtTokenGenerator;

	public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
		IJwtTokenGenerator jwtTokenGenerator)
	{
		_db = db;
		_userManager = userManager;
		_roleManager = roleManager;
		_jwtTokenGenerator = jwtTokenGenerator;
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