using CF_User.Data.TO;
using CF_User.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace CF_User.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AppUserController : ControllerBase
    {
        private readonly IUserService _service;

        private readonly ILogger<AppUserController> _logger;

        public AppUserController(ILogger<AppUserController> logger, IUserService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
        {
            try
            {
                var user = await _service.CreateUserAsync(req.Username, req.Email, req.Password);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            try
            {
                var user = await _service.GetUserByEmailAsync(email);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUserById(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var result = await _service.UpdateUserByIdAsync(id, request.Username, request.Email, request.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            try
            {
                var result = await _service.DeleteUserByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
