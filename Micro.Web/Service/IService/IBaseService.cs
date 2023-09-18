using Micro.Web.Models;

namespace Micro.Web.Service.IService;

public interface IBaseService
{
	Task<ResponseDto?> SendAsync<T>(RequestDto requestDto);
}