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
}