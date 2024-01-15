using System.Diagnostics;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

public class HomeController : Controller
{
	private readonly IProductService _productService;
	private readonly ICartService _cartService;

	private readonly ILogger<HomeController> _logger;

	public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
	{
		_logger = logger;
		_productService = productService;
		_cartService = cartService;
	}

	public async Task<IActionResult> Index()
	{
		List<ProductDto>? list = [];

		ResponseDto? response = await _productService.GetAllProductsAsync();

		if (response != null && response.IsSuccess)
		{
			list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return View(list);
		return View();
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
	}

	[Authorize]
	public async Task<IActionResult> ProductDetails(int productId)
	{
		ProductDto? model = new();

		ResponseDto? response = await _productService.GetProductByIdAsync(productId);

		if (response != null && response.IsSuccess)
		{
			// Using Convert.ToString() to avoid null reference exception
			model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return View(model);
	}
	
	[Authorize]
	[HttpPost]
	[ActionName("ProductDetails")]
	public async Task<IActionResult> ProductDetails(ProductDto productDto)
	{
		CartDto cartDto = new()
		{
			CartHeader = new CartHeaderDto
			{
				UserId = User.Claims.Where(u => u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value
			}
		};

		CartDetailsDto cartDetails = new()
		{
			Count = productDto.Count,
			ProductId = productDto.ProductId,
		};

		List<CartDetailsDto> cartDetailsDtos = [cartDetails];
		cartDto.CartDetails = cartDetailsDtos;

		ResponseDto? response = await _cartService.UpsertCartAsync(cartDto);

		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Item has been added to the Shopping Cart";
			return RedirectToAction(nameof(Index));
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return View(productDto);
	}
}