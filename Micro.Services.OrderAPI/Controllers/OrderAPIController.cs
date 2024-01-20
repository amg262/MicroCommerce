using AutoMapper;
using Micro.MessageBus;
using Micro.Services.OrderAPI.Data;
using Micro.Services.OrderAPI.Models;
using Micro.Services.OrderAPI.Models.Dto;
using Micro.Services.OrderAPI.Service.IService;
using Micro.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Micro.Services.OrderAPI.Controllers;

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