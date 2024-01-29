using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Micro.Web.Models;
using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

/// <summary>
/// Provides base functionalities for making HTTP requests.
/// </summary>
public class BaseService : IBaseService
{
	private readonly IHttpClientFactory _clientFactory;
	private readonly ITokenProvider _tokenProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="BaseService"/> class.
	/// </summary>
	/// <param name="clientFactory">Factory for creating instances of <see cref="HttpClient"/>.</param>
	/// <param name="tokenProvider">Provider for retrieving authentication tokens.</param>
	public BaseService(IHttpClientFactory clientFactory, ITokenProvider tokenProvider)
	{
		_clientFactory = clientFactory;
		_tokenProvider = tokenProvider;
	}

	/// <summary>
	/// Asynchronously sends an HTTP request based on the provided <see cref="RequestDto"/>.
	/// </summary>
	/// <param name="requestDto">The request data transfer object containing request details.</param>
	/// <param name="withBearer">Indicates whether to include a bearer token in the request header.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ResponseDto"/>.</returns>
	public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
	{
		try
		{
			// Create a new HttpClient instance with a named configuration from the IHttpClientFactory
			HttpClient client = _clientFactory.CreateClient("MicroAPI");
			HttpRequestMessage message = new HttpRequestMessage();

			// Set the Accept header based on the ContentType defined in the request DTO
			if (requestDto.ContentType == ContentType.MultipartFormData)
			{
				// Add support for multipart/form-data for file uploads
				message.Headers.Add("Accept", "application/json");
				message.Headers.Add("enctype", "multipart/form-data");
				// message.Headers.Add("Accept", "*/*");
			}
			else
			{
				message.Headers.Add("Accept", "application/json");
			}

			// Include bearer token in the request if required
			if (withBearer)
			{
				var token = _tokenProvider.GetToken();
				message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}

			// Set the request URI and content
			message.RequestUri = new Uri(requestDto.Url);

			if (requestDto.ContentType == ContentType.MultipartFormData)
			{
				var content = new MultipartFormDataContent();

				foreach (var prop in requestDto.Data.GetType().GetProperties())
				{
					var value = prop.GetValue(requestDto.Data);
					if (value is FormFile)
					{
						var file = (FormFile) value;
						if (file != null)
						{
							content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
						}
					}
					else
					{
						content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
					}
				}

				message.Content = content;
			}
			else
			{
				if (requestDto.Data != null)
				{
					message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8,
						"application/json");
				}
			}

			HttpResponseMessage? apiResponse = null;

			// Set the HttpMethod based on the ApiType defined in the request DTO
			message.Method = requestDto.ApiType switch
			{
				ApiType.POST => HttpMethod.Post,
				ApiType.PUT => HttpMethod.Put,
				ApiType.DELETE => HttpMethod.Delete,
				_ => HttpMethod.Get
			};

			// Send the HTTP request asynchronously
			apiResponse = await client.SendAsync(message);

			// Handle different HTTP status codes
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
			// Log the exception details for debugging purposes
			Console.WriteLine(e.ToString());
			if (e.InnerException != null)
			{
				Console.WriteLine($"Inner Exception: {e.InnerException.ToString()}");
			}

			// Return a response DTO indicating the failure
			ResponseDto dto = new() {Message = e.Message.ToString(), IsSuccess = false};
			return dto;
		}
	}
}