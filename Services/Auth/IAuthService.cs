using CF_User.Data.TO.Login;
namespace CF_User.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string username, string password);

        // Create a server-side refresh token for the given username and return the plaintext token
        Task<string> CreateRefreshTokenForUserAsync(string username);

        // Exchange a refresh token for a new access token + rotated refresh token (returns LoginResponse + new refresh token)
        Task<(LoginResponse? response, string? refreshToken)> RefreshTokenAsync(string refreshToken, string? remoteIp, string? userAgent);

        // Revoke a refresh token (mark as revoked)
        Task RevokeRefreshTokenAsync(string refreshToken);

    }
}
