using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Micro.Services.CouponAPI;
using Micro.Services.CouponAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: "Open",
		builder =>
		{
			builder
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader();
		});
});

builder.Services.AddSwaggerGen(option =>
{
	option.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Micro.Services.CouponAPI",
		Version = "v1",
		Description = "This is the Micro.Services.CouponAPI API",
	});
	option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});
	option.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = JwtBearerDefaults.AuthenticationScheme
				}
			},
			Array.Empty<string>()
		}
	});
});
builder.Services.AddHealthChecks()
	.AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
		name: "CouponDb",
		tags: new[] {"ready"}
	);

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// This checks if the db is ready to accept requests and gives a response
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
// This checks if API is live
// This checks if API is live
app.MapHealthChecks("/api/health/live", new HealthCheckOptions
{
	Predicate = _ => false,
});

app.UseHttpsRedirection();
app.UseCors("Open");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
	using var scope = app.Services.CreateScope();
	var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	if (_db.Database.GetPendingMigrations().Any())
	{
		_db.Database.Migrate();
	}
}