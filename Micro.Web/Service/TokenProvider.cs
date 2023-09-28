using Micro.Web.Service.IService;
using Micro.Web.Utility;
using Microsoft.JSInterop;

namespace Micro.Web.Service;

public class TokenProvider : ITokenProvider
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IJSRuntime _jsRuntime;

	public TokenProvider(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime)
	{
		_httpContextAccessor = httpContextAccessor;
		_jsRuntime = jsRuntime;
	}

	public void SetToken(string token)
	{
		_httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
	}

	public string? GetToken()
	{
		string? token = null;
		var hasToken = _httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
		return hasToken == true ? token : null;
	}

	public void ClearToken()
	{
		_httpContextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
	}

	// Secure Context (https) only
	// public async void SetToken(string token)
	// {
	// 	string secretKey = "sNlVYYYxOgtLYVK63yyz"; // This should be securely stored and retrieved
	// 	await _jsRuntime.InvokeVoidAsync("encryptToken", token, secretKey);
	// }
	//
	// public async Task<string?> GetToken()
	// {
	// 	string secretKey = "jOM2hxfOYg4PYEkq93qe"; // This should be securely stored and retrieved
	// 	return await _jsRuntime.InvokeAsync<string>("decryptToken", secretKey);
	// }
	//
	// public async void ClearToken()
	// {
	// 	await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "token");
	// }
}