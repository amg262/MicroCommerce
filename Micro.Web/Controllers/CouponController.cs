using Micro.Web.Models;
using Micro.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

/// <summary>
/// Controller responsible for handling coupon-related operations such as viewing, creating, and deleting coupons.
/// </summary>
public class CouponController : Controller
{
	private readonly ICouponService _couponService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CouponController"/> class.
	/// </summary>
	/// <param name="couponService">Service for handling coupon operations.</param>
	public CouponController(ICouponService couponService)
	{
		_couponService = couponService;
	}

	/// <summary>
	/// Displays the main page for coupon management.
	/// </summary>
	/// <returns>A view of the coupon management main page.</returns>
	public IActionResult Index()
	{
		return View();
	}

	/// <summary>
	/// Retrieves and displays a list of all coupons.
	/// </summary>
	/// <returns>A view displaying a list of coupons.</returns>
	[HttpGet]
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
			TempData["error"] = response?.Message;
		}

		return View(coupons);
	}

	/// <summary>
	/// Displays the view for creating a new coupon.
	/// </summary>
	/// <returns>A view for creating a new coupon.</returns>
	[HttpGet]
	public async Task<IActionResult> CouponCreate()
	{
		return View();
	}

	/// <summary>
	/// Handles the creation of a new coupon.
	/// </summary>
	/// <param name="couponDto">Data transfer object containing coupon details.</param>
	/// <returns>Redirects to the coupon index view if successful, otherwise displays the coupon creation view with an error message.</returns>
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
				TempData["error"] = response?.Message;
			}
		}

		return View(couponDto);
	}

	/// <summary>
	/// Displays the view for deleting a coupon.
	/// </summary>
	/// <param name="couponId">The ID of the coupon to delete.</param>
	/// <returns>A view for deleting the specified coupon.</returns>
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
			TempData["error"] = response?.Message;
		}

		return NotFound();
	}

	/// <summary>
	/// Handles the deletion of a specified coupon.
	/// </summary>
	/// <param name="couponDto">Data transfer object containing the details of the coupon to delete.</param>
	/// <returns>Redirects to the coupon index view if successful, otherwise displays the coupon deletion view with an error message.</returns>
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
			TempData["error"] = response?.Message;
		}

		return View(couponDto);
	}
}