using Micro.Web.Models;
using Micro.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

public class CouponController : Controller
{
	private readonly ICouponService _couponService;

	public CouponController(ICouponService couponService)
	{
		_couponService = couponService;
	}
	
	public IActionResult Index()
	{
		return View();
	}

	// GET
	public async Task<IActionResult> CouponIndex()
	{
		List<CouponDto>? coupons = new();

		ResponseDto? response = await _couponService.GetAllCouponsAsync();

		if (response != null && response.IsSuccess)
		{
			coupons = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(response.Result));
		}

		return View(coupons);
	}
}