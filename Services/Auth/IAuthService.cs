using CF_User.Data.TO.Login;
namespace CF_User.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string username, string password);

    }
}
