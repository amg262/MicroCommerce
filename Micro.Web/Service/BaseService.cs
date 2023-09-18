using System.Net;
using System.Text;
using Newtonsoft.Json;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

public class BaseService : IBaseService
{
	private readonly IHttpClientFactory _clientFactory;

	public BaseService(IHttpClientFactory clientFactory)
	{
		_clientFactory = clientFactory;
	}

	public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
	{
		try
		{
			HttpClient client = _clientFactory.CreateClient("MicroAPI");
			HttpRequestMessage message = new();
			message.Headers.Add("Accept", "application/json");

			message.RequestUri = new Uri(requestDto.Url);
			if (requestDto.Data != null)
			{
				message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8,
					"application/json");
			}

			HttpResponseMessage? apiResponse = null;

			message.Method = requestDto.ApiType switch
			{
				ApiType.POST => HttpMethod.Post,
				ApiType.PUT => HttpMethod.Put,
				ApiType.DELETE => HttpMethod.Delete,
				_ => HttpMethod.Get
			};

			apiResponse = await client.SendAsync(message);

			switch (apiResponse.StatusCode)
			{
				case HttpStatusCode.NotFound:
					return new ResponseDto {DisplayMessage = "Not Found", IsSuccess = false};
				case HttpStatusCode.Forbidden:
					return new ResponseDto {DisplayMessage = "Forbidden Access", IsSuccess = false};
				case HttpStatusCode.Unauthorized:
					return new ResponseDto {DisplayMessage = "Unauthorized", IsSuccess = false};
				case HttpStatusCode.InternalServerError:
					return new ResponseDto {DisplayMessage = "Internal Server Error", IsSuccess = false};
				default:
					var apiContent = await apiResponse.Content.ReadAsStringAsync();
					var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
					return apiResponseDto;
			}
		}
		catch (Exception e)
		{
			ResponseDto dto = new() {DisplayMessage = e.Message.ToString(), IsSuccess = false};
			return dto;
		}
	}

	public Task<ResponseDto?> SendAsync<T>(RequestDto requestDto)
	{
		throw new NotImplementedException();
	}
}