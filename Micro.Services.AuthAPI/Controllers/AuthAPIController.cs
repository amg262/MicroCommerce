using Micro.MessageBus;
using Micro.Services.AuthAPI.Models.Dto;
using Micro.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Micro.Services.AuthAPI.Controllers;

/// <summary>
/// Controller for handling authentication-related actions such as user registration, login, and role assignment.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthAPIController : ControllerBase
{
	private readonly IAuthService _authService;
	private readonly IMessageBus _messageBus;
	private readonly IConfiguration _configuration;
	protected ResponseDto _response;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthAPIController"/>.
	/// </summary>
	/// <param name="authService">Service for authentication operations.</param>
	/// <param name="messageBus">Service for handling message bus operations.</param>
	/// <param name="configuration">Configuration interface for accessing application settings.</param>
	public AuthAPIController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
	{
		_authService = authService;
		_messageBus = messageBus;
		_configuration = configuration;
		_response = new ResponseDto();
	}

	/// <summary>
	/// Registers a new user.
	/// </summary>
	/// <param name="model">Data transfer object containing registration details.</param>
	/// <returns>An HTTP response indicating success or failure of user registration.</returns>
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
	{
		var errorMessage = await _authService.Register(model);

		if (string.IsNullOrEmpty(errorMessage))
		{
			_messageBus.PublishMessage(model.Email,
				_configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"));
			return Ok(_response);
		}

		_response.IsSuccess = false;
		_response.Message = errorMessage;
		return BadRequest(_response);
	}

	/// <summary>
	/// Authenticates a user and logs them in.
	/// </summary>
	/// <param name="model">Data transfer object containing login details.</param>
	/// <returns>An HTTP response indicating success or failure of user login.</returns>
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
	{
		var loginResponse = await _authService.Login(model);

		if (loginResponse.User == null)
		{
			_response.IsSuccess = false;
			_response.Message = "Username or password is incorrect.";
			return BadRequest(_response);
		}

		_response.Result = loginResponse;
		return Ok(_response);
	}

	/// <summary>
	/// Assigns a role to a user.
	/// </summary>
	/// <param name="model">Data transfer object containing user and role details.</param>
	/// <returns>An HTTP response indicating success or failure of role assignment.</returns>
	[HttpPost("AssignRole")]
	public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
	{
		var assignRoleSuccess = await _authService.AssignRole(model.Email, model.Role.ToUpper());

		if (assignRoleSuccess) return Ok(_response);

		_response.IsSuccess = false;
		_response.Message = "Username or password is incorrect.";
		return BadRequest(_response);
	}
}