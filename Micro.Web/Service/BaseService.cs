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
				case HttpStatusCode.PermanentRedirect:
					return new ResponseDto {Message = "Permanent Redirect", IsSuccess = false};
				case HttpStatusCode.BadRequest:
					return new ResponseDto {Message = "Bad Request", IsSuccess = false};
				case HttpStatusCode.Unauthorized:
					return new ResponseDto {Message = "Unauthorized", IsSuccess = false};
				case HttpStatusCode.PaymentRequired:
					return new ResponseDto {Message = "Payment Required", IsSuccess = false};
				case HttpStatusCode.Forbidden:
					return new ResponseDto {Message = "Forbidden Access", IsSuccess = false};
				case HttpStatusCode.NotFound:
					return new ResponseDto {Message = "Not Found", IsSuccess = false};
				case HttpStatusCode.MethodNotAllowed:
					return new ResponseDto {Message = "Method Not Allowed", IsSuccess = false};
				case HttpStatusCode.NotAcceptable:
					return new ResponseDto {Message = "Not Acceptable", IsSuccess = false};
				case HttpStatusCode.ProxyAuthenticationRequired:
					return new ResponseDto {Message = "Proxy Authentication Required", IsSuccess = false};
				case HttpStatusCode.RequestTimeout:
					return new ResponseDto {Message = "Request Timeout", IsSuccess = false};
				case HttpStatusCode.Conflict:
					return new ResponseDto {Message = "Conflict", IsSuccess = false};
				case HttpStatusCode.Gone:
					return new ResponseDto {Message = "Gone", IsSuccess = false};
				case HttpStatusCode.LengthRequired:
					return new ResponseDto {Message = "Length Required", IsSuccess = false};
				case HttpStatusCode.PreconditionFailed:
					return new ResponseDto {Message = "Precondition Failed", IsSuccess = false};
				case HttpStatusCode.RequestEntityTooLarge:
					return new ResponseDto {Message = "Request Entity Too Large", IsSuccess = false};
				case HttpStatusCode.RequestUriTooLong:
					return new ResponseDto {Message = "Request-URI Too Long", IsSuccess = false};
				case HttpStatusCode.UnsupportedMediaType:
					return new ResponseDto {Message = "Unsupported Media Type", IsSuccess = false};
				case HttpStatusCode.RequestedRangeNotSatisfiable:
					return new ResponseDto {Message = "Requested Range Not Satisfiable", IsSuccess = false};
				case HttpStatusCode.ExpectationFailed:
					return new ResponseDto {Message = "Expectation Failed", IsSuccess = false};
				case HttpStatusCode.MisdirectedRequest:
					return new ResponseDto {Message = "Misdirected Request", IsSuccess = false};
				case HttpStatusCode.UnprocessableEntity:
					return new ResponseDto {Message = "Unprocessable Entity", IsSuccess = false};
				case HttpStatusCode.Locked:
					return new ResponseDto {Message = "Locked", IsSuccess = false};
				case HttpStatusCode.FailedDependency:
					return new ResponseDto {Message = "Failed Dependency", IsSuccess = false};
				case HttpStatusCode.TooManyRequests:
					return new ResponseDto {Message = "Too Many Requests", IsSuccess = false};
				case HttpStatusCode.RequestHeaderFieldsTooLarge:
					return new ResponseDto {Message = "Request Header Fields Too Large", IsSuccess = false};
				case HttpStatusCode.UnavailableForLegalReasons:
					return new ResponseDto {Message = "Unavailable For Legal Reasons", IsSuccess = false};
				case HttpStatusCode.InternalServerError:
					return new ResponseDto {Message = "Internal Server Error", IsSuccess = false};
				case HttpStatusCode.NotImplemented:
					return new ResponseDto {Message = "Not Implemented", IsSuccess = false};
				case HttpStatusCode.BadGateway:
					return new ResponseDto {Message = "Bad Gateway", IsSuccess = false};
				case HttpStatusCode.ServiceUnavailable:
					return new ResponseDto {Message = "Service Unavailable", IsSuccess = false};
				case HttpStatusCode.GatewayTimeout:
					return new ResponseDto {Message = "Gateway Timeout", IsSuccess = false};
				case HttpStatusCode.HttpVersionNotSupported:
					return new ResponseDto {Message = "HTTP Version Not Supported", IsSuccess = false};
				case HttpStatusCode.VariantAlsoNegotiates:
					return new ResponseDto {Message = "Variant Also Negotiates", IsSuccess = false};
				case HttpStatusCode.InsufficientStorage:
					return new ResponseDto {Message = "Insufficient Storage", IsSuccess = false};
				case HttpStatusCode.LoopDetected:
					return new ResponseDto {Message = "Loop Detected", IsSuccess = false};
				case HttpStatusCode.NotExtended:
					return new ResponseDto {Message = "Not Extended", IsSuccess = false};
				case HttpStatusCode.NetworkAuthenticationRequired:
					return new ResponseDto {Message = "Network Authentication Required", IsSuccess = false};
				case HttpStatusCode.UpgradeRequired:
				case HttpStatusCode.PreconditionRequired:
				case HttpStatusCode.Continue:
				case HttpStatusCode.SwitchingProtocols:
				case HttpStatusCode.Processing:
				case HttpStatusCode.EarlyHints:
				case HttpStatusCode.OK:
				case HttpStatusCode.Created:
				case HttpStatusCode.Accepted:
				case HttpStatusCode.NonAuthoritativeInformation:
				case HttpStatusCode.NoContent:
				case HttpStatusCode.ResetContent:
				case HttpStatusCode.PartialContent:
				case HttpStatusCode.MultiStatus:
				case HttpStatusCode.AlreadyReported:
				case HttpStatusCode.IMUsed:
				case HttpStatusCode.Ambiguous:
				case HttpStatusCode.Moved:
				case HttpStatusCode.Found:
				case HttpStatusCode.RedirectMethod:
				case HttpStatusCode.NotModified:
				case HttpStatusCode.UseProxy:
				case HttpStatusCode.Unused:
				case HttpStatusCode.RedirectKeepVerb:
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