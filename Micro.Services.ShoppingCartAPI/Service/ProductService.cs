using Micro.Services.ShoppingCartAPI.Models.Dto;
using Micro.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Micro.Services.ShoppingCartAPI.Service;

public class ProductService : IProductService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public ProductService(IHttpClientFactory clientFactory)
	{
		_httpClientFactory = clientFactory;
	}

	public async Task<IEnumerable<ProductDto>> GetProducts()
	{
		var client = _httpClientFactory.CreateClient("Product");
		var response = await client.GetAsync($"/api/product");
		var apiContent = await response.Content.ReadAsStringAsync();
		var dto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
		return dto.IsSuccess
			? JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(dto.Result) ?? string.Empty)
			: new List<ProductDto>();
	}
}