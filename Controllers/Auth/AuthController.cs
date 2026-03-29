using CF_User.Data.TO.Login;
using CF_User.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CF_User.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        // Constructor for AuthController, which takes an IAuthService and ILogger<AuthController> as dependencies.
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login endpoint called for username: {Username}", request.Username);

            try
            {
                _logger.LogDebug("Attempting authentication for username: {Username}", request.Username);
                var response = await _authService.LoginAsync(request.Username, request.Password);

                if (response == null)
                {
                    _logger.LogWarning("Login failed for username: {Username} - Invalid credentials", request.Username);
                    return Unauthorized("Invalid credentials");
                }

                _logger.LogInformation("Login successful for username: {Username}, role: {Role}", request.Username, response.Role);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}. Exception: {Message}", request.Username, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
