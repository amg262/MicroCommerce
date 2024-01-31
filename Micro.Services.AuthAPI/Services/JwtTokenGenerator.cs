using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Micro.Services.AuthAPI.Models;
using Micro.Services.AuthAPI.Services.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Micro.Services.AuthAPI.Services;

/// <summary>
/// Service responsible for generating JWT (JSON Web Token) for user authentication.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
	private readonly JwtOptions _jwtOptions;

	/// <summary>
	/// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
	/// </summary>
	/// <param name="jwtOptions">Configuration options for JWT.</param>
	public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
	{
		_jwtOptions = jwtOptions.Value;
	}

	/// <summary>
	/// Generates a JWT token for a given application user and their roles.
	/// </summary>
	/// <param name="applicationUser">The user for whom the token is being generated.</param>
	/// <param name="roles">The roles of the user to be included in the token.</param>
	/// <returns>A JWT token string.</returns>
	public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

		// Inside token we can add claims, storing email ID, name, phone number in Key Value Pair
		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Email, applicationUser.Email),
			new(JwtRegisteredClaimNames.Name, applicationUser.UserName),
			// Sub (Subject) claim is used to store the user's ID.
			new(JwtRegisteredClaimNames.Sub, applicationUser.Id),
		};
		// Adding role claims to the list of claims.
		claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

		// Preparing the security token descriptor with issuer, audience, subject, expiration, and signing credentials.
		var tokenDescriptor = new SecurityTokenDescriptor()
		{
			Audience = _jwtOptions.Audience,
			Issuer = _jwtOptions.Issuer,
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddDays(7),
			SigningCredentials =
				new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
		};

		try
		{
			// Generating the token using the token handler and descriptor.
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}
}