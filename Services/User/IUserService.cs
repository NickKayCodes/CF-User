using CF_User.Model;


namespace CF_User.Services.User

{
    public interface IUserService
    {
        Task<AppUser> CreateUserAsync(string username, string email, string password);
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<String> DeleteUserByIdAsync(Guid id);
        Task<String> UpdateUserByIdAsync(Guid id, string? username, string? email, string? password);
    }
}
