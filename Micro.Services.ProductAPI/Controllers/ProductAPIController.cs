﻿using AutoMapper;
using Micro.Services.ProductAPI.Data;
using Micro.Services.ProductAPI.Models;
using Micro.Services.ProductAPI.Models.Dto;
using Micro.Services.ProductAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.ProductAPI.Controllers;

/// <summary>
/// API controller for managing products. Includes functionality for CRUD operations on products.
/// </summary>
[Route("api/product")]
[ApiController]
// Uncomment the line below to enable authorization for this controller.
// [Authorize]
public class ProductAPIController : ControllerBase
{
	private readonly AppDbContext _db;
	private ResponseDto _response;
	private readonly IMapper _mapper;

	/// <summary>
	/// Constructor for ProductAPIController.
	/// </summary>
	/// <param name="db">Database context for accessing product data.</param>
	/// <param name="mapper">AutoMapper instance to map between DTOs and entities.</param>
	public ProductAPIController(AppDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
		_response = new ResponseDto();
	}

	/// <summary>
	/// Retrieves all products.
	/// </summary>
	/// <returns>A list of all products.</returns>
	[HttpGet]
	public async Task<ResponseDto> Get()
	{
		try
		{
			IEnumerable<Product> products = await _db.Products.ToListAsync();
			_response.Result = _mapper.Map<IEnumerable<Product>>(products); // Map the Products to a ProductDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}

	/// <summary>
	/// Retrieves a specific product by its ID.
	/// </summary>
	/// <param name="id">The ID of the product.</param>
	/// <returns>The requested product.</returns>
	[HttpGet]
	[Route("{id:int}")]
	public async Task<ResponseDto> Get(int id)
	{
		try
		{
			Product? product = await _db.Products.FirstOrDefaultAsync(i => i.ProductId == id);
			_response.Result = _mapper.Map<ProductDto>(product); // Map the Product to a ProductDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}


	/// <summary>
	/// Creates a new product.
	/// </summary>
	/// <param name="productDto">The product data to create.</param>
	/// <returns>The result of the creation operation.</returns>
	[HttpPost]
	[Authorize(Roles = SD.RoleAdmin)] // Restrict this action to Admin roles only.
	public async Task<ResponseDto> Post([FromForm] ProductDto productDto)
	{
		try
		{
			Product product = _mapper.Map<Product>(productDto);
			_db.Products.Add(product);
			await _db.SaveChangesAsync();

			if (productDto.Image != null)
			{
				string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
				string filePath = @"wwwroot\ProductImages\" + fileName;

				//I have added the if condition to remove the any image with same name if that exist in the folder by any change
				var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);
				FileInfo file = new FileInfo(directoryLocation);
				if (file.Exists)
				{
					file.Delete();
				}

				var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
				await using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
				{
					await productDto.Image.CopyToAsync(fileStream);
				}

				var baseUrl =
					$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
				product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
				product.ImageLocalPath = filePath;
			}
			else
			{
				product.ImageUrl = "https://placehold.co/600x400";
			}

			_db.Products.Update(product);
			await _db.SaveChangesAsync();
			_response.Result = _mapper.Map<ProductDto>(product);
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	/// <summary>
	/// Updates an existing product.
	/// </summary>
	/// <param name="productDto">The updated product data.</param>
	/// <returns>The result of the update operation.</returns>
	[HttpPut]
	[Authorize(Roles = SD.RoleAdmin)] // Restrict this action to Admin roles only.
	public async Task<ResponseDto> Put([FromForm] ProductDto productDto)
	{
		try
		{
			Product product = _mapper.Map<Product>(productDto); // Map the ProductDto to a Product

			if (productDto.Image != null)
			{
				if (!string.IsNullOrEmpty(product.ImageLocalPath))
				{
					var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
					FileInfo file = new FileInfo(oldFilePathDirectory);
					if (file.Exists)
					{
						file.Delete();
					}
				}

				string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
				string filePath = @"wwwroot\ProductImages\" + fileName;
				var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
				await using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
				{
					await productDto.Image.CopyToAsync(fileStream);
				}

				var baseUrl =
					$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
				product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
				product.ImageLocalPath = filePath;
			}

			_db.Products.Update(product);
			await _db.SaveChangesAsync();

			_response.Result = _mapper.Map<ProductDto>(product); // Map the Product to a ProductDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}

	/// <summary>
	/// Deletes a product by its ID.
	/// </summary>
	/// <param name="id">The ID of the product to delete.</param>
	/// <returns>The result of the deletion operation.</returns>
	[HttpDelete]
	[Route("{id:int}")]
	[Authorize(Roles = SD.RoleAdmin)] // Restrict this action to Admin roles only.
	public async Task<ResponseDto> Delete(int id)
	{
		try
		{
			var product = _db.Products.FirstOrDefault(i => i.ProductId == id); // Map the ProductDto to a Product

			if (!string.IsNullOrEmpty(product.ImageLocalPath))
			{
				var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
				FileInfo file = new FileInfo(oldFilePathDirectory);
				if (file.Exists)
				{
					file.Delete();
				}
			}

			_db.Products.Remove(product);
			await _db.SaveChangesAsync();

			_response.Result = _mapper.Map<ProductDto>(product); // Map the Product to a ProductDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}
}