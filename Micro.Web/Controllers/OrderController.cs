using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

/// <summary>
/// Controller responsible for handling order-related requests such as viewing and updating order details.
/// </summary>
public class OrderController : Controller
{
	private readonly IOrderService _orderService;

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderController"/> class.
	/// </summary>
	/// <param name="orderService">Service for handling order operations.</param>
	public OrderController(IOrderService orderService)
	{
		_orderService = orderService;
	}

	/// <summary>
	/// Displays the order index view.
	/// </summary>
	/// <returns>The order index view.</returns>
	public IActionResult OrderIndex()
	{
		return View();
	}

	/// <summary>
	/// Displays the details of a specific order.
	/// </summary>
	/// <param name="orderId">The ID of the order to display.</param>
	/// <returns>The order detail view for the specified order ID.</returns>
	[HttpGet]
	public async Task<IActionResult> OrderDetail(int orderId)
	{
		OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
		string userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

		var response = await _orderService.GetOrder(orderId);
		if (response != null && response.IsSuccess)
		{
			orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
		}

		if (!User.IsInRole(SD.RoleAdmin) && userId != orderHeaderDto.UserId)
		{
			return NotFound();
		}

		return View(orderHeaderDto);
	}

	/// <summary>
	/// Retrieves all orders, optionally filtered by status and/or user ID.
	/// </summary>
	/// <param name="status">Optional status to filter orders.</param>
	/// <returns>A JSON response containing the list of orders.</returns>
	[HttpGet]
	public IActionResult GetAll(string? status)
	{
		IEnumerable<OrderHeaderDto> list;
		string? userId = "";
		if (!User.IsInRole(SD.RoleAdmin))
		{
			userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
		}

		ResponseDto? response = _orderService.GetAllOrder(userId).GetAwaiter().GetResult();
		if (response != null && response.IsSuccess)
		{
			list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));

			switch (status)
			{
				case "approved":
					list = list.Where(u => u.Status == SD.Status_Approved);
					break;
				case "readyforpickup":
					list = list.Where(u => u.Status == SD.Status_ReadyForPickup);
					break;
				case "cancelled":
					list = list.Where(u => u.Status == SD.Status_Cancelled || u.Status == SD.Status_Refunded);
					break;
				default:
					break;
			}
		}
		else
		{
			list = new List<OrderHeaderDto>();
		}

		return Json(new {data = list});
	}

	/// <summary>
	/// Updates the status of an order to 'Ready for Pickup'.
	/// </summary>
	/// <param name="orderId">The ID of the order to update.</param>
	/// <returns>Redirects to the order detail view if successful; otherwise, returns to the current view with an error.</returns>
	[HttpPost("OrderReadyForPickup")]
	public async Task<IActionResult> OrderReadyForPickup(int orderId)
	{
		var response = await _orderService.UpdateOrderStatus(orderId, SD.Status_ReadyForPickup);
		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Status updated successfully";
			return RedirectToAction(nameof(OrderDetail), new {orderId = orderId});
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}

	/// <summary>
	/// Completes an order by updating its status to 'Completed'.
	/// </summary>
	/// <param name="orderId">The ID of the order to complete.</param>
	/// <returns>Redirects to the order detail view if successful; otherwise, returns to the current view with an error.</returns>
	[HttpPost("CompleteOrder")]
	public async Task<IActionResult> CompleteOrder(int orderId)
	{
		var response = await _orderService.UpdateOrderStatus(orderId, SD.Status_Completed);
		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Status updated successfully";
			return RedirectToAction(nameof(OrderDetail), new {orderId = orderId});
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}

	/// <summary>
	/// Cancels an order by updating its status to 'Cancelled'.
	/// </summary>
	/// <param name="orderId">The ID of the order to cancel.</param>
	/// <returns>Redirects to the order detail view if successful; otherwise, returns to the current view with an error.</returns>
	[HttpPost("CancelOrder")]
	public async Task<IActionResult> CancelOrder(int orderId)
	{
		var response = await _orderService.UpdateOrderStatus(orderId, SD.Status_Cancelled);
		if (response != null && response.IsSuccess)
		{
			TempData["success"] = "Status updated successfully";
			return RedirectToAction(nameof(OrderDetail), new {orderId = orderId});
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}
}