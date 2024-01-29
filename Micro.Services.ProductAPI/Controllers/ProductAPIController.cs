using AutoMapper;
using Micro.Services.CouponAPI.Utility;
using Micro.Services.ProductAPI.Data;
using Micro.Services.ProductAPI.Models;
using Micro.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.ProductAPI.Controllers;

[Route("api/product")]
[ApiController]
// [Authorize]
// ControllerBase is a controller without view support
public class ProductAPIController : ControllerBase
{
	private readonly AppDbContext _db;
	private ResponseDto _response;
	private readonly IMapper _mapper;

	public ProductAPIController(AppDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
		_response = new ResponseDto();
	}

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


	[HttpPost]
	[Authorize(Roles = SD.RoleAdmin)]
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

	[HttpPut]
	[Authorize(Roles = SD.RoleAdmin)]
	public async Task<ResponseDto> Put(ProductDto productDto)
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

	[HttpDelete]
	[Route("{id:int}")]
	[Authorize(Roles = SD.RoleAdmin)]
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