using Micro.Web.Utility;

namespace Micro.Web.Models;

public class RequestDto
{
	public ApiType ApiType { get; set; } = ApiType.GET; // Use our enum
	public string Url { get; set; } = "";
	public object Data { get; set; } = new object();
	public string AccessToken { get; set; } = "";
}