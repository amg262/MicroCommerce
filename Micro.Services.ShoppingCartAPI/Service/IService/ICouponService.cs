using Micro.Services.ShoppingCartAPI.Models.Dto;

namespace Micro.Services.ShoppingCartAPI.Service.IService;

public interface ICouponService
{
	Task<CouponDto> GetCoupon(string code);
}