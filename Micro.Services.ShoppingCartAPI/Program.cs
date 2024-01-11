using AutoMapper;
using Micro.Services.ShoppingCartAPI.Data;
using Micro.Services.ShoppingCartAPI;
using Micro.Services.ShoppingCartAPI.Extensions;
using Micro.Services.ShoppingCartAPI.Service;
using Micro.Services.ShoppingCartAPI.Service.IService;
using Micro.Services.ShoppingCartAPI.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();

SD.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"];
SD.CouponAPIBase = builder.Configuration["ServiceUrls:CouponAPI"];

builder.Services.AddHttpClient("Product", u => u.BaseAddress = new Uri(SD.ProductAPIBase));
builder.Services.AddHttpClient("Coupon", u => u.BaseAddress = new Uri(SD.CouponAPIBase));

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
		Title = "Micro.Services.ShoppingCartAPI",
		Version = "v1",
		Description = "This is the Micro.Services.ShoppingCart API",
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
		name: "ShoppingCartDb-check",
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