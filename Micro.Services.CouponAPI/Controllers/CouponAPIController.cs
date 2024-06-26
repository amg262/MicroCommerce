﻿using AutoMapper;
using Micro.Services.CouponAPI.Data;
using Micro.Services.CouponAPI.Models;
using Micro.Services.CouponAPI.Models.Dto;
using Micro.Services.CouponAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.CouponAPI.Controllers;

[Route("api/coupon")]
[ApiController]
[Authorize]
// ControllerBase is a controller without view support
public class CouponAPIController : ControllerBase
{
	private readonly AppDbContext _db;
	private ResponseDto _response;
	private readonly IMapper _mapper;

	public CouponAPIController(AppDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
		_response = new ResponseDto();
	}

	[HttpGet]
	public ResponseDto Get()
	{
		try
		{
			IEnumerable<Coupon> coupons = _db.Coupons.ToList();
			_response.Result = _mapper.Map<IEnumerable<CouponDto>>(coupons); // Map the Coupons to a CouponDto
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
	public ResponseDto Get(int id)
	{
		try
		{
			Coupon coupon = _db.Coupons.First(i => i.CouponId == id);
			_response.Result = _mapper.Map<CouponDto>(coupon);
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}

	[HttpGet]
	[Route("GetByCode/{code}")]
	public async Task<ResponseDto> Get(string code)
	{
		try
		{
			Coupon coupon = await _db.Coupons.FirstAsync(u => u.CouponCode.ToLower() == code.ToLower());
			_response.Result = _mapper.Map<CouponDto>(coupon);
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
	public async Task<ResponseDto> Post([FromBody] CouponDto couponDto)
	{
		try
		{
			Coupon coupon = _mapper.Map<Coupon>(couponDto);
			_db.Coupons.Add(coupon);
			await _db.SaveChangesAsync();

			// Create the coupon in Stripe
			var options = new Stripe.CouponCreateOptions
			{
				AmountOff = (long) (couponDto.DiscountAmount * 100),
				Name = couponDto.CouponCode,
				Currency = "usd",
				Id = couponDto.CouponCode,
			};
			var service = new Stripe.CouponService();
			await service.CreateAsync(options);

			_response.Result = _mapper.Map<CouponDto>(coupon);
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
	public async Task<ResponseDto> Put([FromBody] CouponDto couponDto)
	{
		try
		{
			Coupon coupon = _mapper.Map<Coupon>(couponDto);
			_db.Coupons.Update(coupon);
			await _db.SaveChangesAsync();

			_response.Result = _mapper.Map<CouponDto>(coupon);
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
			Coupon coupon = _db.Coupons.First(i => i.CouponId == id);
			_db.Coupons.Remove(coupon);
			await _db.SaveChangesAsync();
			var service = new Stripe.CouponService();
			await service.DeleteAsync(coupon.CouponCode);
	
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}
}