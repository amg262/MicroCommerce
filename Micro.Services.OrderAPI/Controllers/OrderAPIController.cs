﻿using AutoMapper;
using Micro.MessageBus;
using Micro.Services.OrderAPI.Data;
using Micro.Services.OrderAPI.Models;
using Micro.Services.OrderAPI.Models.Dto;
using Micro.Services.OrderAPI.Service.IService;
using Micro.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Micro.Services.OrderAPI.Controllers;

/// <summary>
/// API Controller for handling order-related operations such as retrieving, creating, updating, and validating orders.
/// </summary>
[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
	public ResponseDto _response;
	private IMapper _mapper;
	private readonly AppDbContext _db;
	private IProductService _productService;
	private readonly IConfiguration _configuration;
	private readonly IMessageBus _messageBus;

	/// <summary>
	/// Initializes a new instance of the <see cref="OrderAPIController"/> class.
	/// </summary>
	/// <param name="db">Database context for accessing order data.</param>
	/// <param name="productService">Service for handling product-related operations.</param>
	/// <param name="mapper">AutoMapper for object-object mapping.</param>
	/// <param name="configuration">Configuration for accessing application settings.</param>
	/// <param name="messageBus">Message bus for publishing messages.</param>
	public OrderAPIController(AppDbContext db, IProductService productService, IMapper mapper,
		IConfiguration configuration, IMessageBus messageBus)
	{
		_db = db;
		this._response = new ResponseDto();
		_productService = productService;
		_mapper = mapper;
		_configuration = configuration;
		_messageBus = messageBus;
	}

	/// <summary>
	/// Retrieves orders for a specific user or all orders if requested by an admin.
	/// </summary>
	/// <param name="userId">Optional user ID for filtering orders. If empty and requested by an admin, all orders are retrieved.</param>
	/// <returns>A response DTO containing a list of orders.</returns>
	[Authorize]
	[HttpGet("GetOrders")]
	public ResponseDto? Get(string? userId = "")
	{
		try
		{
			IEnumerable<OrderHeader> objList;
			if (User.IsInRole(SD.RoleAdmin))
			{
				objList = _db.OrderHeaders.Include(u => u.OrderDetails).OrderByDescending(u => u.OrderHeaderId)
					.ToList();
			}
			else
			{
				objList = _db.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId)
					.OrderByDescending(u => u.OrderHeaderId).ToList();
			}

			_response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	/// <summary>
	/// Retrieves a specific order by its ID.
	/// </summary>
	/// <param name="id">The ID of the order to retrieve.</param>
	/// <returns>A response DTO containing the order details.</returns>
	[Authorize]
	[HttpGet("GetOrder/{id:int}")]
	public ResponseDto? Get(int id)
	{
		try
		{
			OrderHeader orderHeader = _db.OrderHeaders.Include(u => u.OrderDetails).First(u => u.OrderHeaderId == id);
			_response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	/// <summary>
	/// Creates a new order based on the provided cart data.
	/// </summary>
	/// <param name="cartDto">Data transfer object containing cart details.</param>
	/// <returns>A response DTO indicating the result of the order creation.</returns>
	[Authorize]
	[HttpPost("CreateOrder")]
	public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
	{
		try
		{
			OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
			orderHeaderDto.OrderTime = DateTime.Now;
			orderHeaderDto.Status = SD.Status_Pending;
			orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

			OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
			await _db.SaveChangesAsync();

			orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
			_response.Result = orderHeaderDto;
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	/// <summary>
	/// Updates the status of an existing order.
	/// </summary>
	/// <param name="orderId">The ID of the order to update.</param>
	/// <param name="newStatus">The new status for the order.</param>
	/// <returns>A response DTO indicating the result of the update operation.</returns>
	[Authorize]
	[HttpPost("UpdateOrderStatus/{orderId:int}")]
	public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
	{
		try
		{
			OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderId);
			if (orderHeader != null)
			{
				if (newStatus == SD.Status_Cancelled)
				{
					//we will give refund
					var options = new RefundCreateOptions
					{
						Reason = RefundReasons.RequestedByCustomer,
						PaymentIntent = orderHeader.PaymentIntentId
					};

					var service = new RefundService();
					Refund refund = await service.CreateAsync(options);
				}

				orderHeader.Status = newStatus;
				await _db.SaveChangesAsync();
			}
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
		}

		return _response;
	}

	/// <summary>
	/// Creates a Stripe session for processing payment.
	/// </summary>
	/// <param name="stripeRequestDto">Data transfer object containing Stripe request details.</param>
	/// <returns>A response DTO with Stripe session details.</returns>
	[Authorize]
	[HttpPost("CreateStripeSession")]
	public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
	{
		try
		{
			var options = new SessionCreateOptions
			{
				SuccessUrl = stripeRequestDto.ApprovedUrl,
				CancelUrl = stripeRequestDto.CancelUrl,
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				Discounts = new List<SessionDiscountOptions>(),
			};

			var discounts = new List<SessionDiscountOptions>()
			{
				new()
				{
					Coupon = stripeRequestDto.OrderHeader.CouponCode
				}
			};

			foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long) (item.Price * 100), // $20.99 -> 2099
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Name
						}
					},
					Quantity = item.Count
				};

				options.LineItems.Add(sessionLineItem);
			}

			if (stripeRequestDto.OrderHeader.Discount > 0)
			{
				options.Discounts = discounts;
			}

			var service = new SessionService();
			Session session = await service.CreateAsync(options);
			stripeRequestDto.StripeSessionUrl = session.Url;
			OrderHeader orderHeader =
				_db.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
			orderHeader.StripeSessionId = session.Id;
			await _db.SaveChangesAsync();
			_response.Result = stripeRequestDto;
		}
		catch (Exception ex)
		{
			_response.Message = ex.Message;
			_response.IsSuccess = false;
		}

		return _response;
	}

	/// <summary>
	/// Validates the Stripe session for an order.
	/// </summary>
	/// <param name="orderHeaderId">The ID of the order header for Stripe session validation.</param>
	/// <returns>A response DTO indicating whether the Stripe session is validated.</returns>
	[Authorize]
	[HttpPost("ValidateStripeSession")]
	public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
	{
		try
		{
			OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);

			var service = new SessionService();
			Session session = await service.GetAsync(orderHeader.StripeSessionId);

			var paymentIntentService = new PaymentIntentService();
			PaymentIntent paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

			if (paymentIntent.Status == SD.Status_Succeeded.ToLower())
			{
				//then payment was successful
				orderHeader.PaymentIntentId = paymentIntent.Id;
				orderHeader.Status = SD.Status_Approved;
				await _db.SaveChangesAsync();

				RewardsDto rewardsDto = new()
				{
					OrderId = orderHeader.OrderHeaderId,
					RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
					UserId = orderHeader.UserId
				};
				// Sending to Service Bus
				string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
				await _messageBus.PublishMessage(rewardsDto, topicName);

				_response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
			}
		}
		catch (Exception ex)
		{
			_response.Message = ex.Message;
			_response.IsSuccess = false;
		}

		return _response;
	}
}