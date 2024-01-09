using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

public class HomeController : Controller
{
	private readonly IProductService _productService;

	private readonly ILogger<HomeController> _logger;

	public HomeController(ILogger<HomeController> logger, IProductService productService)
	{
		_logger = logger;
		_productService = productService;
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

	public IActionResult Details()
	{
		throw new NotImplementedException();
	}
}