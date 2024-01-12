using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

/// <summary>
/// Provides service operations for managing shopping cart functionalities.
/// </summary>
public class CartService : ICartService
{
	private readonly IBaseService _baseService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CartService"/> class.
	/// </summary>
	/// <param name="baseService">The base service used for HTTP requests.</param>
	public CartService(IBaseService baseService)
	{
		_baseService = baseService;
	}

	/// <summary>
	/// Retrieves a shopping cart for a specific user.
	/// </summary>
	/// <param name="userId">The user's identifier for whom the cart is retrieved.</param>
	/// <returns>The user's shopping cart as a ResponseDto object.</returns>
	public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Url = SD.CartAPIBase + "/api/cart/GetCart/" + userId,
		});
	}

	/// <summary>
	/// Inserts or updates a shopping cart.
	/// </summary>
	/// <param name="cartDto">The shopping cart data transfer object containing cart details.</param>
	/// <returns>The result of the upsert operation as a ResponseDto object.</returns>
	public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = cartDto,
			Url = SD.CartAPIBase + "/api/cart/CartUpsert"
		});
	}

	/// <summary>
	/// Removes an item from the shopping cart.
	/// </summary>
	/// <param name="cartDetailsId">The identifier of the cart detail to remove.</param>
	/// <returns>The result of the removal operation as a ResponseDto object.</returns>
	public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.POST,
			Data = cartDetailsId,
			Url = SD.CartAPIBase + "/api/cart/RemoveCart"
		});
	}

	/// <summary>
	/// Applies a coupon to the shopping cart.
	/// </summary>
	/// <param name="cartDto">The shopping cart data transfer object to apply the coupon to.</param>
	/// <returns>The result of the coupon application as a ResponseDto object.</returns>
	public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
	{
		return await _baseService.SendAsync(new RequestDto()
		{
			ApiType = ApiType.PUT,
			Data = cartDto,
			Url = SD.CartAPIBase + "/api/cart/ApplyCoupon",
		});
	}
}