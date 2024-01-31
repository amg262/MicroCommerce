using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

/// <summary>
/// Service class for handling orders, including creation, updating, and retrieval of order information.
/// </summary>
/// <param name="baseService">Base service for HTTP requests handling.<see cref="BaseService"/></param>
public class OrderService(IBaseService baseService) : IOrderService
{
	/// <summary>
	/// Creates an order asynchronously based on the provided cart details.
	/// </summary>
	/// <param name="cartDto">Data transfer object containing cart details.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO with order creation result.</returns>
	public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
	{
		return await baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = cartDto,
			Url = SD.OrderAPIBase + "/api/order/CreateOrder"
		});
	}

	/// <summary>
	/// Creates a Stripe payment session asynchronously for the provided Stripe request.
	/// </summary>
	/// <param name="stripeRequestDto">Data transfer object containing Stripe request details.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO with Stripe session creation result.</returns>
	public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
	{
		return await baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = stripeRequestDto,
			Url = SD.OrderAPIBase + "/api/order/CreateStripeSession"
		});
	}

	/// <summary>
	/// Validates a Stripe session asynchronously based on the provided order header ID.
	/// </summary>
	/// <param name="orderHeaderId">The ID of the order header for Stripe session validation.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO with Stripe session validation result.</returns>
	public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
	{
		return await baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = orderHeaderId,
			Url = SD.OrderAPIBase + "/api/order/ValidateStripeSession"
		});
	}

	/// <summary>
	/// Retrieves all orders asynchronously, optionally filtered by a user ID.
	/// </summary>
	/// <param name="userId">Optional user ID to filter orders. If null, all orders are retrieved.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO with the list of orders.</returns>
	public async Task<ResponseDto?> GetAllOrder(string? userId)
	{
		return await baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.OrderAPIBase + "/api/order/GetOrders/" + userId
		});
	}

	/// <summary>
	/// Retrieves a specific order asynchronously based on the provided order ID.
	/// </summary>
	/// <param name="orderId">The ID of the order to retrieve.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO with the requested order details.</returns>
	public async Task<ResponseDto?> GetOrder(int orderId)
	{
		return await baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.GET,
			Url = SD.OrderAPIBase + "/api/order/GetOrder/" + orderId
		});
	}

	/// <summary>
	/// Updates the status of an order asynchronously.
	/// </summary>
	/// <param name="orderId">The ID of the order to update.</param>
	/// <param name="newStatus">The new status to be set for the order.</param>
	/// <returns>A task that represents the asynchronous operation, returning a response DTO indicating the result of the update.</returns>
	public async Task<ResponseDto?> UpdateOrderStatus(int orderId, string newStatus)
	{
		return await baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = newStatus,
			Url = SD.OrderAPIBase + "/api/order/UpdateOrderStatus/" + orderId
		});
	}
}