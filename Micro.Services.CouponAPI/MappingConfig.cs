using AutoMapper;
using Micro.Services.CouponAPI.Models;
using Micro.Services.CouponAPI.Models.Dto;

namespace Micro.Services.CouponAPI;

public class MappingConfig
{
	public static MapperConfiguration RegisterMaps()
	{
		// Mapping from Coupon to CouponDto and back because the CouponDto is what we want to return to the client
		MapperConfiguration config = new(cfg =>
		{
			cfg.CreateMap<Coupon, CouponDto>().ReverseMap();
		});

		return config;
	}
}