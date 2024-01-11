using Micro.Services.ShoppingCartAPI.Models.Dto;
using Micro.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Micro.Services.ShoppingCartAPI.Service;

public class CouponService : ICouponService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public CouponService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public async Task<CouponDto> GetCoupon(string code)
	{
		var client = _httpClientFactory.CreateClient("Coupon");
		var response = await client.GetAsync($"/api/coupon/GetByCode/{code}");
		var apiContent = await response.Content.ReadAsStringAsync();
		var dto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
		return dto.IsSuccess
			? JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(dto.Result))
			: new CouponDto();
	}
}