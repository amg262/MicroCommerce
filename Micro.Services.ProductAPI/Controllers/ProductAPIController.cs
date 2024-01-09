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
			IEnumerable<Product> coupons = await _db.Products.ToListAsync();
			_response.Result = _mapper.Map<IEnumerable<Product>>(coupons); // Map the Products to a ProductDto
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
			Product? coupon = await _db.Products.FirstOrDefaultAsync(i => i.ProductId == id);
			_response.Result = _mapper.Map<ProductDto>(coupon); // Map the Product to a ProductDto
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
	public async Task<ResponseDto> Post([FromBody] ProductDto couponDto)
	{
		try
		{
			Product coupon = _mapper.Map<Product>(couponDto); // Map the ProductDto to a Product
			await _db.Products.AddAsync(coupon);
			await _db.SaveChangesAsync();

			_response.Result = _mapper.Map<ProductDto>(coupon); // Map the Product to a ProductDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}

	[HttpPut]
	[Authorize(Roles = SD.RoleAdmin)]
	public async Task<ResponseDto> Put([FromBody] ProductDto couponDto)
	{
		try
		{
			Product coupon = _mapper.Map<Product>(couponDto); // Map the ProductDto to a Product
			_db.Products.Update(coupon);
			await _db.SaveChangesAsync();

			_response.Result = _mapper.Map<ProductDto>(coupon); // Map the Product to a ProductDto
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
			var coupon = _db.Products.FirstOrDefault(i => i.ProductId == id); // Map the ProductDto to a Product
			_db.Products.Remove(coupon);
			await _db.SaveChangesAsync();

			_response.Result = _mapper.Map<ProductDto>(coupon); // Map the Product to a ProductDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}
}