using AutoMapper;
using Micro.Services.ProductAPI.Models;
using Micro.Services.ProductAPI.Models.Dto;

namespace Micro.Services.ProductAPI;

public class MappingConfig
{
	public static MapperConfiguration RegisterMaps()
	{
		// Mapping from Coupon to CouponDto and back because the CouponDto is what we want to return to the client
		MapperConfiguration config = new(cfg => { cfg.CreateMap<Product, ProductDto>().ReverseMap(); });
		return config;
	}
}