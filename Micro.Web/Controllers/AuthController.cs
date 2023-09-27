using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

public class AuthController : Controller
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
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

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
		else
		{
			ModelState.AddModelError("Custom Error", responseDto.Message);
		}
		return View(dto);
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
		ResponseDto assignRole;

		if (result != null && result.IsSuccess)
		{
			if (!string.IsNullOrEmpty(dto.Role))
			{
				dto.Role = SD.RoleCustomer;
			}

			assignRole = await _authService.AssignRoleAsync(dto);

			if (assignRole != null && assignRole.IsSuccess)
			{
				TempData["success"] = "Registration Successful.";
				return RedirectToAction(nameof(Login));
			}
		}

		var roleList = new List<SelectListItem>
		{
			new SelectListItem {Text = SD.RoleAdmin, Value = SD.RoleAdmin},
			new SelectListItem {Text = SD.RoleCustomer, Value = SD.RoleCustomer}
		};

		// Save the role list to a ViewBag property instead of ViewModel
		ViewBag.RoleList = roleList;
		return View(dto);
	}

	public IActionResult Logout()
	{
		return View();
	}
}