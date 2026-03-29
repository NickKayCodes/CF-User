using CF_User.Model;
using CF_User.Model.enums;


namespace CF_User.Services.User

{
    public interface IUserService
    {
        Task<AppUser> CreateUserAsync(string username, string email, string password, UserRole role);
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<String> DeleteUserByIdAsync(Guid id);
        Task<String> UpdateUserByIdAsync(Guid id, string? username, string? email, string? password, UserRole? role, List<Privilege>? privileges);
    }
}
