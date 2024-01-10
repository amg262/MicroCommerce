namespace Micro.Services.ShoppingCartAPI.Models.Dto;

// So we can have the same response format for all our APIs and objects we return
public class ResponseDto
{
	public object? Result { get; set; }
	public bool IsSuccess { get; set; } = true;
	public string Message { get; set; } = "";
}