namespace Micro.Web.Service.IService;

public interface ITokenProvider
{
	// Secure Context (https) only
	// void SetToken(string token);
	// Task<string?> GetToken();
	// void ClearToken();
	void SetToken(string token);
	string? GetToken();
	void ClearToken();
}