using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CF_User.Data;
using CF_User.Data.TO.Login;
using CF_User.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace CF_User.Services.Auth
{
    public class AuthService : IAuthService
    {   
        private readonly AppDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        /*** Constructor for AuthService.
         * @param dbContext The database context for accessing user data.
         * @param jwtOptions The JWT settings for token generation.
         * @param logger The logger for logging authentication events.
         */
        public AuthService(AppDbContext dbContext, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Login attempt for username: {Username}", username);

            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Privileges)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for username: {Username}", username);
                    return null;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for username: {Username}", username);
                    return null;
                }

                _logger.LogDebug("Password verified for user: {Username}, generating JWT token", username);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Role, user.Role.ToString()),
                        }.Concat(user.Privileges.Select(p => new Claim("privilege", p.Privilege.ToString())))
                    ),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    ),
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var response = new LoginResponse
                {
                    Token = tokenHandler.WriteToken(token),
                    Username = user.Username,
                    Role = user.Role.ToString(),
                    Privileges = user.Privileges.Select(p => p.Privilege.ToString()),
                };

                _logger.LogInformation("Login successful for user: {Username}, token generated with role: {Role} and privilege count: {PrivilegeCount}", 
                    username, user.Role.ToString(), user.Privileges.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for username: {Username}. Exception: {Message}", username, ex.Message);
                throw;
            }
        }
    }
}
