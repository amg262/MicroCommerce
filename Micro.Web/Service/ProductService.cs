using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

/// <summary>
/// Provides services related to product management, including retrieving, creating, updating, and deleting products.
/// </summary>
public class ProductService : IProductService
{
	private readonly IBaseService _baseService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProductService"/> class.
	/// </summary>
	/// <param name="baseService">The base service for handling HTTP requests.</param>
	public ProductService(IBaseService baseService)
	{
		_baseService = baseService;
	}

	/// <summary>
	/// Asynchronously retrieves all products.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO containing all products.</returns>
	public async Task<ResponseDto?> GetAllProductsAsync()
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.ProductAPIBase + "/api/product/",
		});
	}

	/// <summary>
	/// Asynchronously retrieves a product by its ID.
	/// </summary>
	/// <param name="id">The ID of the product to retrieve.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO for the specified product.</returns>
	public async Task<ResponseDto?> GetProductByIdAsync(int id)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.ProductAPIBase + "/api/product/" + id,
		});
	}

	/// <summary>
	/// Asynchronously creates a new product.
	/// </summary>
	/// <param name="productDto">The product DTO containing the information for the new product.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO indicating the result of the creation operation.</returns>
	public async Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = productDto,
			Url = SD.ProductAPIBase + "/api/product",
			ContentType = ContentType.MultipartFormData
		});
	}

	/// <summary>
	/// Asynchronously updates an existing product.
	/// </summary>
	/// <param name="productDto">The product DTO containing the updated information for the product.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO indicating the result of the update operation.</returns>
	public async Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.PUT,
			Data = productDto,
			Url = SD.ProductAPIBase + "/api/product",
			ContentType = ContentType.MultipartFormData
		});
	}

	/// <summary>
	/// Asynchronously deletes a product by its ID.
	/// </summary>
	/// <param name="id">The ID of the product to delete.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO indicating the result of the deletion operation.</returns>
	public async Task<ResponseDto?> DeleteProductAsync(int id)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.DELETE,
			Url = SD.ProductAPIBase + "/api/product/" + id,
		});
	}
}