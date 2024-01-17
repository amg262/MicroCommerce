using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

public class OrderService : IOrderService
{
	private readonly IBaseService _baseService;

	public OrderService(IBaseService baseService)
	{
		_baseService = baseService;
	}

	public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
	{
		try
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = ApiType.POST,
				Data = cartDto,
				Url = SD.OrderAPIBase + "/api/order/CreateOrder",
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException);
			throw;
		}
	}
}