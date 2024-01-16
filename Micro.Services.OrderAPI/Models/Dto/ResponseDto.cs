namespace Micro.Services.OrderAPI.Models.Dto;

// So we can have the same response format for all our APIs and objects we return
public class ResponseDto
{
	public object? Result { get; set; }
	public bool IsSuccess { get; set; } = true;
	public string Message { get; set; } = "";

	public override string ToString()
	{
		return $"{nameof(Result)}: {Result}, {nameof(IsSuccess)}: {IsSuccess}, {nameof(Message)}: {Message}";
	}
}