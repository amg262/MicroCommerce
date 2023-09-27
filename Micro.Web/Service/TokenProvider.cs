using Micro.Web.Service.IService;
using Micro.Web.Utility;

namespace Micro.Web.Service;

public class TokenProvider : ITokenProvider
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public TokenProvider(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public void SetToken(string token)
	{
		_httpContextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
	}

	public string? GetToken()
	{
		throw new NotImplementedException();
	}

	public void ClearToken()
	{
		throw new NotImplementedException();
	}
}