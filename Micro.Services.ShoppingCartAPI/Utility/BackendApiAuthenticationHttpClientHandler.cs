using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Micro.Services.ShoppingCartAPI.Utility;

/// <summary>
/// A custom HTTP client handler that adds an authentication token to outgoing requests.
/// This class extends DelegatingHandler and is used to intercept HTTP requests in order to add a bearer token for authentication purposes.
/// Delegating Handler are similar to middleware, but they are used to modify the request before it is sent to the server
/// they are on clientside, we can leverage DelegatingHandler to add the token to the request header to authenticate the request to other APIS
/// </summary>
public class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
{
	// HttpContextAccessor is used to access the HttpContext in a class that is not a part of the HTTP request pipeline.
	private readonly IHttpContextAccessor _accessor;

	/// <summary>
	/// Initializes a new instance of the BackendApiAuthenticationHttpClientHandler class.
	/// </summary>
	/// <param name="accessor">The HttpContextAccessor to access the current HTTP context.</param>
	public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor accessor)
	{
		_accessor = accessor;
	}

	/// <summary>
	/// Sends an HTTP request asynchronously after adding an authentication token to the request header if available.
	/// </summary>
	/// <param name="request">The HTTP request message to send.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The HTTP response message.</returns>
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// string access_token = _accessor.HttpContext.User.FindFirst("access_token")?.Value;
		// Retrieve the access token from the current HttpContext.
		var token = await _accessor?.HttpContext.GetTokenAsync("access_token");

		if (!string.IsNullOrEmpty(token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		// Call the base implementation of SendAsync to continue processing the HTTP request.
		return await base.SendAsync(request, cancellationToken);
	}
}