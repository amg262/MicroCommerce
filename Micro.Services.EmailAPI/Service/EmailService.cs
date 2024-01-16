using System.Text;
using Micro.Services.EmailAPI.Data;
using Micro.Services.EmailAPI.Models;
using Micro.Services.EmailAPI.Models.Dto;
using Micro.Services.EmailAPI.Utility;
using Microsoft.EntityFrameworkCore;

namespace Micro.Services.EmailAPI.Service;

public class EmailService : IEmailService
{
	// We can't use AppDbContext here because its a scoped service and cant be injected into a singleton service
	private readonly DbContextOptions<AppDbContext> _dbOptions;

	public EmailService(DbContextOptions<AppDbContext> dbOptions)
	{
		_dbOptions = dbOptions;
	}

	public async Task EmailCartAndLog(CartDto cartDto)
	{
		StringBuilder message = new StringBuilder();

		message.AppendLine("<br/>Cart Email Requested ");
		message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
		message.Append("<br/>");
		message.Append("<ul>");
		foreach (var item in cartDto.CartDetails)
		{
			message.Append("<li>");
			message.Append(item.Product.Name + " x " + item.Count);
			message.Append("</li>");
		}

		message.Append("</ul>");

		await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);
	}

	public async Task RegisterUserEmailAndLog(string email)
	{
		string message = "User Registration Successful. <br/> Email : " + email;
		await LogAndEmail(message, SD.EmailAdmin);
	}

	private async Task<bool> LogAndEmail(string message, string email)
	{
		try
		{
			EmailLogger emailLog = new()
			{
				Email = email,
				EmailSent = DateTime.Now,
				Message = message
			};
			await using var db = new AppDbContext(_dbOptions);
			await db.EmailLoggers.AddAsync(emailLog);
			await db.SaveChangesAsync();
			return true;
		}
		catch (Exception ex)
		{
			return false;
		}
	}
}