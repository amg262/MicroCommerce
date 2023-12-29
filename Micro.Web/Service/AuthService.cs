﻿using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

public class AuthService : IAuthService
{
	private readonly IBaseService _baseService;

	public AuthService(IBaseService baseService)
	{
		_baseService = baseService;
	}

	public async Task<ResponseDto?> RegisterUserAsync(RegistrationRequestDto registrationRequestDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = registrationRequestDto,
			Url = SD.AuthAPIBase + "/api/auth/register",
		});
	}

	public async Task<ResponseDto?> LoginUserAsync(LoginRequestDto loginRequestDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = loginRequestDto,
			Url = SD.AuthAPIBase + "/api/auth/login",
		}, withBearer: false);
	}

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