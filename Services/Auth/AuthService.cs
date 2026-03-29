using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CF_User.Data;
using CF_User.Data.TO.Login;
using CF_User.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CF_User.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        /*** Constructor for AuthService.
         * @param dbContext The database context for accessing user data.
         * @param jwtOptions The JWT settings for token generation.
         */
        public AuthService(AppDbContext dbContext, IOptions<JwtSettings> jwtOptions)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            var user = await _dbContext.Users
                .Include(u => u.Privileges)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }
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
            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                Username = user.Username,
                Role = user.Role.ToString(), // Assuming Role is an enum, convert it to string
                Privileges = user.Privileges.Select(p => p.Privilege.ToString()), // Access Privilege property from UserPrivilegeEntity
            };
        }
    }
}
