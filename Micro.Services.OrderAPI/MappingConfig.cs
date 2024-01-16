using AutoMapper;
using Micro.Services.OrderAPI.Models;
using Micro.Services.OrderAPI.Models.Dto;

namespace Micro.Services.OrderAPI;

public class MappingConfig
{
	public static MapperConfiguration RegisterMaps()
	{
		// Mapping from Coupon to CouponDto and back because the CouponDto is what we want to return to the client
		var mappingConfig = new MapperConfiguration(config =>
		{
			// Mapping fields from Order, Cart DTOs that have same field but different name
			config.CreateMap<OrderHeaderDto, CartHeaderDto>()
				.ForMember(dest => dest.CartTotal, u => u.MapFrom(src => src.OrderTotal)).ReverseMap();
			config.CreateMap<CartDetailsDto, OrderDetailsDto>()
				.ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.Product.Name))
				.ForMember(dest => dest.Price, u => u.MapFrom(src => src.Product.Price));
			config.CreateMap<OrderDetailsDto, CartDetailsDto>();
			config.CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();
			config.CreateMap<OrderDetails, OrderDetails>().ReverseMap();

		});
		return mappingConfig;
	}
}