namespace Micro.Web.Models;

public class LoginResponseDto
{
	public UserDto User { get; set; }
	public string Token { get; set; }

	public override string ToString()
	{
		return $"{nameof(User)}: {User}, {nameof(Token)}: {Token}";
	}
}