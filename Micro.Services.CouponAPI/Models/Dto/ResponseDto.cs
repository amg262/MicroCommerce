namespace Micro.Services.CouponAPI.Models.Dto;

// So we can have the same response format for all our APIs and objects we return
public record ResponseDto
{
	public object? Result { get; set; }
	public bool IsSuccess { get; set; } = true;
	public string DisplayMessage { get; set; } = "";
}