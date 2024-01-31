using Micro.Web.Models;
using Micro.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

/// <summary>
/// Controller responsible for handling product-related operations such as 
/// viewing, creating, editing, and deleting products.
/// </summary>
public class ProductController : Controller
{
	private readonly IProductService _productService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProductController"/> class.
	/// </summary>
	/// <param name="productService">Service for handling product-related operations.</param>
	public ProductController(IProductService productService)
	{
		_productService = productService;
	}

	/// <summary>
	/// Retrieves and displays a list of all products.
	/// </summary>
	/// <returns>The view for the product index page.</returns>
	public async Task<IActionResult> ProductIndex()
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
	/// Displays the view for creating a new product.
	/// </summary>
	/// <returns>The view for creating a product.</returns>
	public async Task<IActionResult> ProductCreate()
	{
		return View();
	}

	/// <summary>
	/// Handles the submission of a new product.
	/// </summary>
	/// <param name="model">The product to create.</param>
	/// <returns>Redirects to the product index page if successful; otherwise, returns to the product create view.</returns>
	[HttpPost]
	public async Task<IActionResult> ProductCreate(ProductDto model)
	{
		if (!ModelState.IsValid) return View(model);

		ResponseDto? response = await _productService.CreateProductAsync(model);

		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Product created successfully";
			return RedirectToAction(nameof(ProductIndex));
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return View(model);
	}

	/// <summary>
	/// Displays the view for deleting a product.
	/// </summary>
	/// <param name="productId">The ID of the product to delete.</param>
	/// <returns>The view for deleting a product if found; otherwise, returns a NotFound result.</returns>
	public async Task<IActionResult> ProductDelete(int productId)
	{
		ResponseDto? response = await _productService.GetProductByIdAsync(productId);

		if (response != null && response.IsSuccess)
		{
			ProductDto? model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
			return View(model);
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return NotFound();
	}

	/// <summary>
	/// Handles the deletion of a product.
	/// </summary>
	/// <param name="productDto">The product to delete.</param>
	/// <returns>Redirects to the product index page if successful; otherwise, returns to the product delete view.</returns>
	[HttpPost]
	public async Task<IActionResult> ProductDelete(ProductDto productDto)
	{
		ResponseDto? response = await _productService.DeleteProductAsync(productDto.ProductId);

		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Product deleted successfully";
			return RedirectToAction(nameof(ProductIndex));
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return View(productDto);
	}

	/// <summary>
	/// Displays the view for editing a product.
	/// </summary>
	/// <param name="productId">The ID of the product to edit.</param>
	/// <returns>The view for editing a product if found; otherwise, returns a NotFound result.</returns>
	[HttpGet]
	public async Task<IActionResult> ProductEdit(int productId)
	{
		ResponseDto? response = await _productService.GetProductByIdAsync(productId);

		if (response != null && response.IsSuccess)
		{
			ProductDto? model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
			return View(model);
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return NotFound();
	}

	/// <summary>
	/// Handles the submission of an edited product.
	/// </summary>
	/// <param name="productDto">The product with updated details.</param>
	/// <returns>Redirects to the product index page if successful; otherwise, returns to the product edit view.</returns>
	[HttpPost]
	public async Task<IActionResult> ProductEdit(ProductDto productDto)
	{
		if (!ModelState.IsValid) return View(productDto);

		ResponseDto? response = await _productService.UpdateProductAsync(productDto);

		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Product updated successfully";
			return RedirectToAction(nameof(ProductIndex));
		}
		else
		{
			TempData["error"] = response?.Message;
		}

		return View(productDto);
	}
}