using Micro.Web.Models;

namespace Micro.Web.Service.IService;

public interface IOrderService
{
	Task<ResponseDto?> CreateOrder(CartDto cartDto);
}