using System.ComponentModel.DataAnnotations;

namespace Micro.Web.Models;

public class LoginRequestDto
{
	[Required] public string Username { get; set; }
	[Required] public string Password { get; set; }
}