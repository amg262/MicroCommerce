using Micro.Services.EmailAPI.Data;
using Micro.Services.EmailAPI.Extensions;
using Micro.Services.EmailAPI.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.UseAzureServiceBusConsumer();
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