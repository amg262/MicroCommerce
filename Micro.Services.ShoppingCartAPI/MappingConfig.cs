using AutoMapper;
using Micro.Services.ShoppingCartAPI.Models;
using Micro.Services.ShoppingCartAPI.Models.Dto;

namespace Micro.Services.ShoppingCartAPI;

public class MappingConfig
{
	public static MapperConfiguration RegisterMaps()
	{
		// Mapping from Coupon to CouponDto and back because the CouponDto is what we want to return to the client
		var mappingConfig = new MapperConfiguration(config =>
		{
			config.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
			config.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
		});
		return mappingConfig;
	}
}