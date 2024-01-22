using Micro.Web.Utility;

namespace Micro.Web.Models;

public class RequestDto
{
	public ApiType ApiType { get; set; } = ApiType.GET; // Use our enum
	public string Url { get; set; } = "";
	public object Data { get; set; } = new object();
	public string AccessToken { get; set; } = "";
	public ContentType ContentType { get; set; } = ContentType.Json; // from SD

	public override string ToString()
	{
		return
			$"{nameof(ApiType)}: {ApiType}, {nameof(Url)}: {Url}, {nameof(Data)}: {Data}, " +
			$"{nameof(AccessToken)}: {AccessToken}, {nameof(ContentType)}: {ContentType}";
	}
}