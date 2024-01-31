using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

namespace Micro.Services.CouponAPI.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/> to set up health checks and authentication.
/// </summary>
public static class WebApplicationBuilderExtensions
{
	/// <summary>
	/// Maps health check endpoints for the web application.
	/// </summary>
	/// <param name="app">The <see cref="WebApplication"/> to map health checks to.</param>
	/// <returns>The modified <see cref="WebApplication"/>.</returns>
	public static WebApplication MapAppHealthChecks(this WebApplication app)
	{
		app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
		{
			Predicate = check => check.Tags.Contains("ready"),
			ResponseWriter = async (context, report) =>
			{
				var result = JsonSerializer.Serialize(
					new
					{
						status = report.Status.ToString(),
						checks = report.Entries.Select(entry => new
						{
							name = entry.Key,
							status = entry.Value.Status.ToString(),
							exception = entry.Value.Exception != null ? entry.Value.Exception.Message : "none",
							duration = entry.Value.Duration.ToString()
						})
					}
				);

				context.Response.ContentType = MediaTypeNames.Application.Json;
				await context.Response.WriteAsync(result);
			}
		});

		app.MapHealthChecks("/api/health/live", new HealthCheckOptions
		{
			Predicate = _ => false,
		});

		return app;
	}

	/// <summary>
	/// Adds JWT authentication to the application.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to add authentication to.</param>
	/// <returns>The modified <see cref="WebApplicationBuilder"/>.</returns>
	public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
	{
		var apiSettings = builder.Configuration.GetSection("ApiSettings");
		var secret = apiSettings.GetValue<string>("Secret");
		var issuer = apiSettings.GetValue<string>("Issuer");
		var audience = apiSettings.GetValue<string>("Audience");
		var key = Encoding.ASCII.GetBytes(secret);

		builder.Services.AddAuthentication(o =>
		{
			o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(o =>
		{
			o.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = issuer,
				ValidateAudience = true,
				ValidAudience = audience,
				// RequireExpirationTime = true,
				// ValidateLifetime = true,
				// ClockSkew = TimeSpan.Zero
			};
		});
		builder.Services.AddAuthorization();

		return builder;
	}
}