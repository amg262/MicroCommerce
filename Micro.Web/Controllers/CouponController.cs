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
		else
		{
			TempData["error"] = response?.DisplayMessage;
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
			ResponseDto? response = await _couponService.CreateCouponAsync(couponDto);

			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "Coupon created successfully";
				return RedirectToAction(nameof(CouponIndex));
			}
			else
			{
				TempData["error"] = response?.DisplayMessage;
			}
		}

		return View(couponDto);
	}

	[HttpGet]
	public async Task<IActionResult> CouponDelete(int couponId)
	{
		ResponseDto? response = await _couponService.GetCouponByIdAsync(couponId);

		if (response != null && response.IsSuccess)
		{
			CouponDto dto = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
			return View(dto);
		}
		else
		{
			TempData["error"] = response?.DisplayMessage;
		}

		return NotFound();
	}

	[HttpPost]
	public async Task<IActionResult> CouponDelete(CouponDto couponDto)
	{
		ResponseDto? response = await _couponService.DeleteCouponAsync(couponDto.CouponId);

		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Coupon deleted successfully";
			return RedirectToAction(nameof(CouponIndex));
		}
		else
		{
			TempData["error"] = response?.DisplayMessage;
		}

		return View(couponDto);
	}
}