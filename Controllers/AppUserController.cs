using CF_User.Data.TO.Create;
using CF_User.Data.TO.Update;
using CF_User.Model.enums;
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
            _logger.LogInformation("CreateUser endpoint called with username: {Username}, email: {Email}", req.Username, req.Email);

            try
            {
                _logger.LogInformation("Attempting to parse role: {Role}", req.Role);

                if (!Enum.TryParse<UserRole>(req.Role, ignoreCase: true, out var role))
                {
                    _logger.LogWarning("Invalid role provided: {Role}", req.Role);
                    return BadRequest("Invalid role provided");
                }

                _logger.LogInformation("Role parsed successfully: {Role}", role);
                var response = await _service.CreateUserAsync(req.Username, req.Email, req.Password, role);

                _logger.LogInformation("User created successfully with ID: {UserId}, username: {Username}", response.Id, response.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}. Exception: {Message}", req.Email, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            _logger.LogInformation("GetUserByEmail endpoint called with email: {Email}", email);

            try
            {
                _logger.LogInformation("Fetching user by email: {Email}", email);
                var response = await _service.GetUserByEmailAsync(email);

                _logger.LogInformation("User retrieved successfully with ID: {UserId}, email: {Email}", response.Id, response.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}. Exception: {Message}", email, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUserById(Guid id, [FromBody] UpdateUserRequest request)
        {
            _logger.LogInformation("UpdateUserById endpoint called with ID: {UserId}", id);

            try
            {
                _logger.LogInformation("Update request - Username: {Username}, Email: {Email}, Role: {Role}", request.Username, request.Email, request.Role);

                UserRole? parsedRole = null;

                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    _logger.LogInformation("Attempting to parse role: {Role}", request.Role);

                    if (!Enum.TryParse<UserRole>(request.Role, true, out var r))
                    {
                        _logger.LogWarning("Invalid role provided during update: {Role}", request.Role);
                        return BadRequest("Invalid role provided");
                    }

                    parsedRole = r;
                    _logger.LogInformation("Role parsed successfully: {Role}", parsedRole);
                }

                _logger.LogInformation("Calling UpdateUserByIdAsync with privileges count: {PrivilegeCount}", request.Privileges?.Count ?? 0);
                var response = await _service.UpdateUserByIdAsync(
                    id,
                    request.Username,
                    request.Email,
                    request.Password,
                    parsedRole,
                    request.Privileges
                );

                _logger.LogInformation("User updated successfully with ID: {UserId}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}. Exception: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            _logger.LogInformation("DeleteUserById endpoint called with ID: {UserId}", id);

            try
            {
                _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);
                var response = await _service.DeleteUserByIdAsync(id);

                _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}. Exception: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
