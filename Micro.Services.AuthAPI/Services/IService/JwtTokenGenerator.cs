using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Micro.Services.AuthAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Micro.Services.AuthAPI.Services.IService;

public class JwtTokenGenerator : IJwtTokenGenerator
{
	private readonly JwtOptions _jwtOptions;

	public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
	{
		_jwtOptions = jwtOptions.Value;
	}

	public string GenerateToken(ApplicationUser applicationUser)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

		// Inside token we can add claims, storing email ID, name, phone number in Key Value Pair
		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Email, applicationUser.Email),
			new(JwtRegisteredClaimNames.Name, applicationUser.UserName),
			// Sub for Subject -> ID of the user
			new(JwtRegisteredClaimNames.Sub, applicationUser.Id),
		};

		var tokenDescriptor = new SecurityTokenDescriptor()
		{
			Audience = _jwtOptions.Audience,
			Issuer = _jwtOptions.Issuer,
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddDays(7),
			SigningCredentials =
				new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}
}