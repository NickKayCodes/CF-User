using CF_User.Model;

namespace CF_User.Repo.User
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email);
        Task AddUserAsync(AppUser user);
        Task DeleteUserAsync(AppUser existingUser);
        Task<AppUser> GetByIdAsync(Guid id);
        Task<AppUser> UpdateUserbyIdAsync(AppUser user);
    }
}
