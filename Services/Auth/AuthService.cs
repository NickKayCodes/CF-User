using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using CF_User.Data;
using CF_User.Data.TO.Login;
using CF_User.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Linq;

// additional usings for hashing
using System;

namespace CF_User.Services.Auth
{
    public class AuthService : IAuthService
    {   
        private readonly AppDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;
        private readonly CF_User.Services.PH.IPH _hasher;

        /*** Constructor for AuthService.
         * @param dbContext The database context for accessing user data.
         * @param jwtOptions The JWT settings for token generation.
         * @param logger The logger for logging authentication events.
         */
        public AuthService(AppDbContext dbContext, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger, CF_User.Services.PH.IPH hasher)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
            _hasher = hasher;
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Login attempt for username: {Username}", username);
            _logger.LogWarning("Username received: '{username}'", username);
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Privileges)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for username: {Username}", username);
                    return null;
                }

                var verify = _hasher.VerifyHashedPassword(user.PasswordHash, password);
                // log verification outcome for debugging (do not log plaintext)
                _logger.LogDebug("Password verification for user {Username}: {Result}. StoredHashPrefix: {HashPrefix}, HashLength: {HashLength}",
                    username,
                    verify == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success ? "Success" : "Failed",
                    user.PasswordHash != null ? user.PasswordHash.Substring(0, Math.Min(8, user.PasswordHash.Length)) : string.Empty,
                    user.PasswordHash?.Length ?? 0);
                if (verify != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                {
                    _logger.LogWarning("Login failed: Invalid password for username: {Username}", username);
                    return null;
                }

                _logger.LogInformation("Password verified for user: {Username}, generating JWT token", username);

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

        // generate a cryptographically secure random token (base64url)
        private static string GenerateRandomToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(bytes);
            // base64url-safe
            token = token.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return token;
        }

        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public async Task<string> CreateRefreshTokenForUserAsync(string username)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            if (user == null) throw new Exception("User not found");

            var plainToken = GenerateRandomToken();
            var tokenHash = HashToken(plainToken);

            var refresh = new CF_User.Model.Auth.RefreshToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Revoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(refresh);
            await _dbContext.SaveChangesAsync();

            return plainToken;
        }

        public async Task<(LoginResponse? response, string? refreshToken)> RefreshTokenAsync(string refreshToken, string? remoteIp, string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return (null, null);

            var hash = HashToken(refreshToken);

            var existing = await _dbContext.RefreshTokens
                .Include(r => r.User)
                    .ThenInclude(u => u.Privileges)
                .FirstOrDefaultAsync(r => r.TokenHash == hash);

            if (existing == null || existing.Revoked || existing.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token invalid or expired");
                return (null, null);
            }

            var user = existing.User!;

            // generate new access token
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
            var accessToken = tokenHandler.WriteToken(token);

            // rotate refresh token: create a new one and mark existing revoked
            var newPlain = GenerateRandomToken();
            var newHash = HashToken(newPlain);

            existing.Revoked = true;
            existing.ReplacedByTokenHash = newHash;

            var newRefresh = new CF_User.Model.Auth.RefreshToken
            {
                TokenHash = newHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Revoked = false,
                CreatedAt = DateTime.UtcNow,
                RemoteIpAddress = remoteIp,
                UserAgent = userAgent
            };

            _dbContext.RefreshTokens.Add(newRefresh);
            await _dbContext.SaveChangesAsync();

            var response = new LoginResponse
            {
                Token = accessToken,
                Username = user.Username,
                Role = user.Role.ToString(),
                Privileges = user.Privileges.Select(p => p.Privilege.ToString()),
            };

            _logger.LogInformation("Refresh token rotated for user: {Username}", user.Username);

            return (response, newPlain);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var hash = HashToken(refreshToken);
            var existing = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
            if (existing == null) return;

            existing.Revoked = true;
            await _dbContext.SaveChangesAsync();
        }
    }
}
