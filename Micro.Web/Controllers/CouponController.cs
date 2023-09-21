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

	[HttpGet]
	public async Task<IActionResult> CouponCreate()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> CouponCreate(CouponDto couponDto)
	{
		if (ModelState.IsValid)
		{
			ResponseDto? responseDto = await _couponService.CreateCouponAsync(couponDto);

			if (responseDto != null && responseDto.IsSuccess)
			{
				return RedirectToAction(nameof(CouponIndex));
			}
		}

		return View(couponDto);
	}
}