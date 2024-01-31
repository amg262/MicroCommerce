using Micro.Web.Service.IService;
using Micro.Web.Utility;
using Microsoft.JSInterop;

namespace Micro.Web.Service;

/// <summary>
/// Provides token management services for authentication purposes.
/// </summary>
public class TokenProvider : ITokenProvider
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	// private readonly IJSRuntime _jsRuntime;

	/// <summary>
	/// Initializes a new instance of the <see cref="TokenProvider"/> class.
	/// </summary>
	/// <param name="httpContextAccessor">Provides access to the HTTP context.</param>
	public TokenProvider(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	/// <summary>
	/// Sets the authentication token in the HTTP context's cookies.
	/// </summary>
	/// <param name="token">The authentication token to be set.</param>
	public void SetToken(string token)
	{
		_httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
	}

	/// <summary>
	/// Retrieves the authentication token from the HTTP context's cookies.
	/// </summary>
	/// <returns>The authentication token if available; otherwise, null.</returns>
	public string? GetToken()
	{
		string? token = null;
		var hasToken = _httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
		return hasToken == true ? token : null;
	}

	/// <summary>
	/// Clears the authentication token from the HTTP context's cookies.
	/// </summary>
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