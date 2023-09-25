using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Micro.Services.CouponAPI.Data;
using Micro.Services.CouponAPI.Models;
using Micro.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Micro.Services.CouponAPI.Controllers;

[Route("api/coupon")]
[ApiController]
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
			_response.Result = _mapper.Map<CouponDto>(coupon); // Map the Coupon to a CouponDto
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
	public ResponseDto Get(string code)
	{
		try
		{
			Coupon coupon = _db.Coupons.First(i =>
				string.Equals(i.CouponCode, code, StringComparison.CurrentCultureIgnoreCase));
			_response.Result = _mapper.Map<CouponDto>(coupon); // Map the Coupon to a CouponDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}

	[HttpPost]
	public ResponseDto Post([FromBody] CouponDto couponDto)
	{
		try
		{
			Coupon coupon = _mapper.Map<Coupon>(couponDto); // Map the CouponDto to a Coupon
			_db.Coupons.Add(coupon);
			_db.SaveChanges();

			_response.Result = _mapper.Map<CouponDto>(coupon); // Map the Coupon to a CouponDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}

	[HttpPut]
	public ResponseDto Put([FromBody] CouponDto couponDto)
	{
		try
		{
			Coupon coupon = _mapper.Map<Coupon>(couponDto); // Map the CouponDto to a Coupon
			_db.Coupons.Update(coupon);
			_db.SaveChanges();

			_response.Result = _mapper.Map<CouponDto>(coupon); // Map the Coupon to a CouponDto
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
	public ResponseDto Delete(int id)
	{
		try
		{
			Coupon coupon = _db.Coupons.First(i => i.CouponId == id); // Map the CouponDto to a Coupon
			_db.Coupons.Remove(coupon);
			_db.SaveChanges();

			_response.Result = _mapper.Map<CouponDto>(coupon); // Map the Coupon to a CouponDto
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.Message = e.Message;
		}

		return _response;
	}
}