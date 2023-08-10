using System;
using System.Collections.Generic;
using System.Linq;
using Micro.Services.CouponAPI.Data;
using Micro.Services.CouponAPI.Models;
using Micro.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Micro.Services.CouponAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
// ControllerBase is a controller without view support
public class CouponAPIController : ControllerBase
{
	private readonly AppDbContext _db;
	private ResponseDto _response;

	public CouponAPIController(AppDbContext db)
	{
		_db = db;
		_response = new ResponseDto();
	}

	[HttpGet]
	public ResponseDto Get()
	{
		try
		{
			IEnumerable<Coupon> coupons = _db.Coupons.ToList();
			_response.Result = coupons;
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.DisplayMessage = e.Message;
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
			_response.Result = coupon;
		}
		catch (Exception e)
		{
			_response.IsSuccess = false;
			_response.DisplayMessage = e.Message;
		}

		return _response;
	}
}