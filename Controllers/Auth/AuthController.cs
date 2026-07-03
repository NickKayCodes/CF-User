using CF_User.Data.TO.Login;
using CF_User.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
                _logger.LogWarning("Username received: '{Username}'", request.Username);
                var response = await _authService.LoginAsync(request.Username, request.Password);

                if (response == null)
                {
                    _logger.LogWarning("Login failed for username: {Username} - Invalid credentials", request.Username);
                    return Unauthorized("Invalid credentials");
                }

                // create refresh token and set as HttpOnly cookie for browser clients
                try
                {
                    var refreshToken = await _authService.CreateRefreshTokenForUserAsync(response.Username);

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // set to true in production when using HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Path = "/"
                    };

                    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create refresh token for user: {Username}", request.Username);
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

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrWhiteSpace(refreshToken)) return BadRequest("Refresh token missing");

                var userAgent = Request.Headers["User-Agent"].ToString();
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                var (response, newRefresh) = await _authService.RefreshTokenAsync(refreshToken, remoteIp, userAgent);

                if (response == null) return Unauthorized("Invalid refresh token");

                // set new refresh token cookie (rotated)
                if (!string.IsNullOrEmpty(newRefresh))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Path = "/"
                    };

                    Response.Cookies.Append("refreshToken", newRefresh, cookieOptions);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrWhiteSpace(refreshToken))
                    return BadRequest("Refresh token missing");

                await _authService.RevokeRefreshTokenAsync(refreshToken);

                return Ok("Refresh token revoked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
