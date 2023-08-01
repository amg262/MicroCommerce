using System;
using System.Collections.Generic;
using System.Linq;
using Micro.Services.CouponAPI.Data;
using Micro.Services.CouponAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Micro.Services.CouponAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
// ControllerBase is a controller without view support
public class CouponAPIController : ControllerBase
{
	private readonly AppDbContext _db;

	public CouponAPIController(AppDbContext db)
	{
		_db = db;
	}

	[HttpGet]
	public object Get()
	{
		try
		{
			IEnumerable<Coupon> coupons = _db.Coupons.ToList();
			return coupons;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error occurred while fetching coupons: {e.Message}");
		}

		return null;
	}
	
	[HttpGet]
	[Route("{id:int}")]
	public object Get(int id)
	{
		try
		{
			Coupon coupons = _db.Coupons.First(i=>i.CouponId == id);
			return coupons;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error occurred while fetching coupons: {e.Message}");
		}

		return null;
	}
}