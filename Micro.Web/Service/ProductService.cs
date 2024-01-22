﻿using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

public class ProductService : IProductService
{
	private readonly IBaseService _baseService;

	public ProductService(IBaseService baseService)
	{
		_baseService = baseService;
	}

	public async Task<ResponseDto?> GetProductAsync(string productCode)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.ProductAPIBase + "/api/product/GetByCode/" + productCode,
		});
	}

	public async Task<ResponseDto?> GetAllProductsAsync()
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.ProductAPIBase + "/api/product/",
		});
	}

	public async Task<ResponseDto?> GetProductByIdAsync(int id)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.ProductAPIBase + "/api/product/" + id,
		});
	}

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

	public async Task<ResponseDto?> DeleteProductAsync(int id)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.DELETE,
			Url = SD.ProductAPIBase + "/api/product/" + id,
		});
	}
}