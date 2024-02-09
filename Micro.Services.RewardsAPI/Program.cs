using Micro.Services.RewardsAPI.Data;
using Micro.Services.RewardsAPI.Extensions;
using Micro.Services.RewardsAPI.Messaging;
using Micro.Services.RewardsAPI.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
	option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddSingleton(new RewardService(optionBuilder.Options));
builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("swagger/v1/swagger.json", "Rewards API");
	c.RoutePrefix = string.Empty;
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.UseAzureServiceBusConsumer();

app.Run();

void ApplyMigration()
{
	using var scope = app?.Services.CreateScope();
	var db = scope?.ServiceProvider.GetRequiredService<AppDbContext>();

	if (db != null && db.Database.GetPendingMigrations().Any())
	{
		db.Database.Migrate();
	}
}