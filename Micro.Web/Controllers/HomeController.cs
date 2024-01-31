using System.Diagnostics;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

/// <summary>
/// Controller responsible for managing the home page, privacy page, error handling, product details, and shopping cart operations.
/// </summary>
public class HomeController : Controller
{
	private readonly IProductService _productService;
	private readonly ICartService _cartService;
	private readonly ILogger<HomeController> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="HomeController"/> class.
	/// </summary>
	/// <param name="logger">Logger for capturing logging information.</param>
	/// <param name="productService">Service for product-related operations.</param>
	/// <param name="cartService">Service for shopping cart-related operations.</param>
	public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
	{
		_logger = logger;
		_productService = productService;
		_cartService = cartService;
	}

	/// <summary>
	/// Displays the home page with a list of products.
	/// </summary>
	/// <returns>The home page view with product listings.</returns>
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
	}

	/// <summary>
	/// Displays the privacy policy page.
	/// </summary>
	/// <returns>The privacy policy view.</returns>
	public IActionResult Privacy()
	{
		return View();
	}

	/// <summary>
	/// Displays the error page with error details.
	/// </summary>
	/// <returns>The error view model.</returns>
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
	}

	/// <summary>
	/// Displays the product details page for a specific product.
	/// This method requires the user to be authorized.
	/// </summary>
	/// <param name="productId">The ID of the product to display.</param>
	/// <returns>The product details view.</returns>
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

	/// <summary>
	/// Handles the action of adding a product to the shopping cart.
	/// This method requires the user to be authorized and is a POST request.
	/// </summary>
	/// <param name="productDto">The product data transfer object containing the product details to be added to the cart.</param>
	/// <returns>Redirects to the Index page if successful; otherwise, returns to the product details page with an error message.</returns>
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