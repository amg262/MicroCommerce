using Micro.Web.Models;
using Micro.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

public class CartController : Controller
{
	private readonly ICartService _cartService;

	public CartController(ICartService cartService)
	{
		_cartService = cartService;
	}

	[Authorize]
	public async Task<IActionResult> CartIndex()
	{
		return View(await LoadCartDtoBasedOnLoggedInUser());
	}

	private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
	{
		// var userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault().Value;
		var userId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
		ResponseDto? responseDto = await _cartService.GetCartByUserIdAsync(userId.Value);

		if (responseDto == null || !responseDto.IsSuccess) return new CartDto();

		CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(responseDto.Result));
		return cartDto;
	}

	public async Task<IActionResult> Remove(int cartDetailsId)
	{
		var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
		ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);

		if (!(response != null & response.IsSuccess)) return View(nameof(CartIndex));

		TempData["success"] = "Cart updated successfully";
		return RedirectToAction(nameof(CartIndex));
	}

	[HttpPost]
	public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
	{
		ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

		if (!(response != null & response.IsSuccess)) return View(nameof(CartIndex));

		TempData["success"] = "Cart updated successfully";
		return RedirectToAction(nameof(CartIndex));
	}

	[HttpPost]
	public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
	{
		cartDto.CartHeader.CouponCode = string.Empty;
		ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

		if (!(response != null & response.IsSuccess)) return View(nameof(CartIndex));

		TempData["success"] = "Cart updated successfully";
		return RedirectToAction(nameof(CartIndex));
	}

	[HttpPost]
	public async Task<IActionResult> EmailCart(CartDto cartDto)
	{
		CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
		cart.CartHeader.Email =
			User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
		ResponseDto? response = await _cartService.EmailCart(cart);

		// ReSharper disable once Mvc.ViewNotResolved
		if (!(response != null & response.IsSuccess)) return View();
		TempData["success"] = "Email will be processed and sent shortly.";
		return RedirectToAction(nameof(CartIndex));
	}
}