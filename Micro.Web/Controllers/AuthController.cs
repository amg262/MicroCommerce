using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

public class AuthController : Controller
{
	private readonly IAuthService _authService;
	private readonly ITokenProvider _tokenProvider;

	public AuthController(IAuthService authService, ITokenProvider tokenProvider)
	{
		_authService = authService;
		_tokenProvider = tokenProvider;
	}

	[HttpGet]
	public IActionResult Login()
	{
		LoginRequestDto loginRequestDto = new();
		return View(loginRequestDto);
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginRequestDto dto)
	{
		ResponseDto responseDto = await _authService.LoginUserAsync(dto);

		if (responseDto != null && responseDto.IsSuccess)
		{
			LoginResponseDto loginResponseDto =
				JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));

			await SignInUser(loginResponseDto);
			_tokenProvider.SetToken(loginResponseDto.Token);

			TempData["success"] = "Login Successful.";
			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
		else
		{
			TempData["error"] = responseDto.Message;
			return View(dto);
		}
	}

	[HttpGet]
	public IActionResult Register()
	{
		var roleList = new List<SelectListItem>
		{
			new SelectListItem {Text = SD.RoleAdmin, Value = SD.RoleAdmin},
			new SelectListItem {Text = SD.RoleCustomer, Value = SD.RoleCustomer}
		};

		// Save the role list to a ViewBag property instead of ViewModel
		ViewBag.RoleList = roleList;
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Register(RegistrationRequestDto dto)
	{
		ResponseDto result = await _authService.RegisterUserAsync(dto);

		if (result != null && result.IsSuccess)
		{
			if (string.IsNullOrEmpty(dto.Role))
			{
				dto.Role = SD.RoleCustomer;
			}

			var assignRole = await _authService.AssignRoleAsync(dto);

			if (assignRole != null && assignRole.IsSuccess)
			{
				TempData["success"] = "Registration Successful.";
				return RedirectToAction(nameof(Login));
			}
		}
		else
		{
			TempData["error"] = result.Message;
		}

		var roleList = new List<SelectListItem>
		{
			new() {Text = SD.RoleAdmin, Value = SD.RoleAdmin},
			new() {Text = SD.RoleCustomer, Value = SD.RoleCustomer}
		};

		// Save the role list to a ViewBag property instead of ViewModel
		ViewBag.RoleList = roleList;
		return View(dto);
	}

	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync();
		_tokenProvider.ClearToken();
		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	private async Task SignInUser(LoginResponseDto loginResponseDto)
	{
		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(loginResponseDto.Token);
		var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
		identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
			jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
		identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
			jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
		identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
			jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));
		identity.AddClaim(new Claim(ClaimTypes.Name,
			jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));

		// Add the role claim to the identity object so that we can use it in the Authorize attribute
		identity.AddClaim(new Claim(ClaimTypes.Role,
			jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));


		var principal = new ClaimsPrincipal(identity);

		await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
		// , new AuthenticationProperties
		// {
		// 	IsPersistent = true, 
		// 	ExpiresUtc = DateTime.UtcNow.AddDays(7)
		// });
	}
}