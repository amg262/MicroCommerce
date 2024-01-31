using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace Micro.Web.Controllers;

/// <summary>
/// Controller responsible for handling cart-related operations such as viewing, modifying, and checking out the cart.
/// </summary>
public class CartController : Controller
{
	private readonly ICartService _cartService;
	private readonly IOrderService _orderService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CartController"/> class.
	/// </summary>
	/// <param name="cartService">Service for handling cart operations.</param>
	/// <param name="orderService">Service for handling order operations.</param>
	public CartController(ICartService cartService, IOrderService orderService)
	{
		_cartService = cartService;
		_orderService = orderService;
	}

	/// <summary>
	/// Displays the cart index view to the authorized user.
	/// </summary>
	/// <returns>A view representing the current state of the user's cart.</returns>
	[Authorize]
	public async Task<IActionResult> CartIndex()
	{
		return View(await LoadCartDtoBasedOnLoggedInUser());
	}

	/// <summary>
	/// Displays the checkout view to the authorized user.
	/// </summary>
	/// <returns>A view for checking out the items in the cart.</returns>
	[Authorize]
	public async Task<IActionResult> Checkout()
	{
		return View(await LoadCartDtoBasedOnLoggedInUser());
	}

	/// <summary>
	/// Handles the confirmation of an order.
	/// </summary>
	/// <param name="orderId">The ID of the order to confirm.</param>
	/// <returns>A view indicating the confirmation status of the order.</returns>
	public async Task<IActionResult> Confirmation(int orderId)
	{
		ResponseDto? response = await _orderService.ValidateStripeSession(orderId);
		if (response != null & response.IsSuccess)
		{
			OrderHeaderDto orderHeaderDto =
				JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
			if (orderHeaderDto.Status == SD.Status_Approved)
			{
				return View(orderId);
			}
		}

		return View(orderId);
	}

	/// <summary>
	/// Processes the checkout operation for the cart.
	/// </summary>
	/// <param name="cartDto">The data transfer object representing the cart.</param>
	/// <returns>A redirect to the Stripe session for payment processing or a view indicating failure.</returns>
	[HttpPost]
	[ActionName("Checkout")]
	public async Task<IActionResult> Checkout(CartDto cartDto)
	{
		CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
		cart.CartHeader.Phone = cartDto.CartHeader.Phone;
		cart.CartHeader.Email = cartDto.CartHeader.Email;
		cart.CartHeader.Name = cartDto.CartHeader.Name;

		var response = await _orderService.CreateOrder(cart);
		OrderHeaderDto orderHeaderDto =
			JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

		if (response != null && response.IsSuccess)
		{
			var domain = $"{Request.Scheme}://{Request.Host.Value}/";
			//get stripe session and redirect to stripe to place order    

			StripeRequestDto stripeRequestDto = new()
			{
				OrderHeader = orderHeaderDto,
				ApprovedUrl = domain + "Cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
				CancelUrl = domain + "Cart/Checkout"
			};

			var stripeResponse = await _orderService.CreateStripeSession(stripeRequestDto);
			StripeRequestDto stripeResponseResult =
				JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));
			Response.Headers.Append("Location", stripeResponseResult.StripeSessionUrl);
			return new StatusCodeResult(303);
		}

		return View();
	}

	/// <summary>
	/// Handles the removal of an item from the cart.
	/// </summary>
	/// <param name="cartDetailsId">The ID of the cart detail item to remove.</param>
	/// <returns>A redirection to the cart index view with a status message.</returns>
	public async Task<IActionResult> Remove(int cartDetailsId)
	{
		var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
		ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);
		if (response != null & response.IsSuccess)
		{
			TempData["success"] = "Cart updated successfully";
			return RedirectToAction(nameof(CartIndex));
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}

	/// <summary>
	/// Applies a coupon to the user's cart.
	/// </summary>
	/// <param name="cartDto">The data transfer object representing the cart to which the coupon is applied.</param>
	/// <returns>A redirection to the cart index view with a status message.</returns>
	[HttpPost]
	public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
	{
		ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
		if (response != null & response.IsSuccess)
		{
			TempData["success"] = "Cart updated successfully";
			return RedirectToAction(nameof(CartIndex));
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}

	/// <summary>
	/// Sends an email representation of the cart to the user's email address.
	/// </summary>
	/// <param name="cartDto">The data transfer object representing the cart to email.</param>
	/// <returns>A redirection to the cart index view with a status message.</returns>
	[HttpPost]
	public async Task<IActionResult> EmailCart(CartDto cartDto)
	{
		CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
		cart.CartHeader.Email =
			User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
		ResponseDto? response = await _cartService.EmailCart(cart);
		if (response != null & response.IsSuccess)
		{
			TempData["success"] = "Email will be processed and sent shortly.";
			return RedirectToAction(nameof(CartIndex));
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}

	/// <summary>
	/// Removes the applied coupon from the user's cart.
	/// </summary>
	/// <param name="cartDto">The data transfer object representing the cart from which the coupon is removed.</param>
	/// <returns>A redirection to the cart index view with a status message.</returns>
	[HttpPost]
	public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
	{
		cartDto.CartHeader.CouponCode = "";
		ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
		if (response != null & response.IsSuccess)
		{
			TempData["success"] = "Cart updated successfully";
			return RedirectToAction(nameof(CartIndex));
		}

		// ReSharper disable once Mvc.ViewNotResolved
		return View();
	}

	/// <summary>
	/// Loads the cart data transfer object based on the currently logged-in user.
	/// </summary>
	/// <returns>A <see cref="CartDto"/> object representing the user's cart.</returns>
	private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
	{
		var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
		ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId);
		if (response != null & response.IsSuccess)
		{
			CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
			return cartDto;
		}

		return new CartDto();
	}
}