using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

public class BaseService : IBaseService
{
	private readonly IHttpClientFactory _clientFactory;
	private readonly ITokenProvider _tokenProvider;

	public BaseService(IHttpClientFactory clientFactory, ITokenProvider tokenProvider)
	{
		_clientFactory = clientFactory;
		_tokenProvider = tokenProvider;
	}

	public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
	{
		try
		{
			HttpClient client = _clientFactory.CreateClient("MicroAPI");
			HttpRequestMessage message = new();


			message.Headers.Add("Accept", "application/json");
			// token
			if (withBearer)
			{
				// 
				var token = _tokenProvider.GetToken();
				message.Headers.Add("Authorization", $"Bearer {token}");
			}

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
					return new ResponseDto {Message = "Not Found", IsSuccess = false};
				case HttpStatusCode.Forbidden:
					return new ResponseDto {Message = "Forbidden Access", IsSuccess = false};
				case HttpStatusCode.Unauthorized:
					return new ResponseDto {Message = "Unauthorized", IsSuccess = false};
				case HttpStatusCode.InternalServerError:
					return new ResponseDto {Message = "Internal Server Error", IsSuccess = false};
				default:
					var apiContent = await apiResponse.Content.ReadAsStringAsync();
					var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
					return apiResponseDto;
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
			if (e.InnerException != null)
			{
				Console.WriteLine($"Inner Exception: {e.InnerException.ToString()}");
			}

			ResponseDto dto = new() {Message = e.Message.ToString(), IsSuccess = false};
			return dto;
		}
	}
}