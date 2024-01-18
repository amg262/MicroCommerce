using AutoMapper;
using Micro.MessageBus;
using Micro.Services.OrderAPI;
using Micro.Services.OrderAPI.Data;
using Micro.Services.OrderAPI.Extensions;
using Micro.Services.OrderAPI.Service.IService;
using Micro.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Stripe;
using ProductService = Micro.Services.OrderAPI.Service.ProductService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMessageBus, MessageBus>();

SD.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"];
Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();


builder.Services.AddHttpClient("Product", u => u.BaseAddress = new Uri(SD.ProductAPIBase))
	.AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();


builder.Services.AddCors(options =>
{
	options.AddPolicy(name: "Open",
		policyBuilder =>
		{
			policyBuilder
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader();
		});
});

builder.Services.AddSwaggerGen(option =>
{
	option.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Micro.Services.OrderAPI",
		Version = "v1",
		Description = "This is the Micro.Services.Order API",
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
		name: "OrderDb-check",
		tags: new[] {"ready"}
	);

// Use extension method to add authentication
builder.AddAppAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseDeveloperExceptionPage();
}

// Use extension method to map health checks
app.MapAppHealthChecks();
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