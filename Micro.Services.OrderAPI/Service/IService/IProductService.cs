using Micro.Services.OrderAPI.Models.Dto;

namespace Micro.Services.OrderAPI.Service.IService;

public interface IProductService
{
	Task<IEnumerable<ProductDto>> GetProducts();
}