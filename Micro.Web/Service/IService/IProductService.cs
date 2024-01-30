using Micro.Web.Models;

namespace Micro.Web.Service.IService;

public interface IProductService
{
	Task<ResponseDto?> GetAllProductsAsync();
	Task<ResponseDto?> GetProductByIdAsync(int id);
	Task<ResponseDto?> CreateProductAsync(ProductDto couponDto);
	Task<ResponseDto?> UpdateProductAsync(ProductDto couponDto);
	Task<ResponseDto?> DeleteProductAsync(int id);
}