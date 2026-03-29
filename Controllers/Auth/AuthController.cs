using CF_User.Data.TO.Login;
using CF_User.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CF_User.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        // Constructor for AuthController, which takes an IAuthService as a dependency.
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Call the LoginAsync method of the IAuthService with the provided username and password.
            var result = await _authService.LoginAsync(request.Username, request.Password);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            return Ok(result);
        }
    }
}
