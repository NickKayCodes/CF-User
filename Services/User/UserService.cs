using CF_User.Model;
using CF_User.Repo.User;
using CF_User.Services.PH;
using Microsoft.AspNetCore.Identity;

namespace CF_User.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IPH _hasher;

        public UserService(IUserRepository repo, IPH hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<AppUser> CreateUserAsync(string username, string email, string password)
        {
            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null)
                throw new Exception("Email already in use");

            var hash = _hasher.HashPassword(password);
            var user = new AppUser(username, email, hash);

            await _repo.AddUserAsync(user);
            return user;
        }

        public async Task<String> DeleteUserByIdAsync(Guid id)
        {
            var existingUser = await _repo.GetByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");

            await _repo.DeleteUserAsync(existingUser);
            return "User deleted successfully";

        }


        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            var existingUser = await _repo.GetByEmailAsync(email);
            if (existingUser == null)
                throw new Exception("Email does not exist");
            return existingUser;
        }

        public async Task<string> UpdateUserByIdAsync(Guid id, string? username, string? email, string? password)
        {
            var existingUser = await _repo.GetByIdAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");

            if (username != null)
                existingUser.SetUsername(username);

            if (email != null)
                existingUser.SetEmail(email);

            if (password != null)
                existingUser.SetPassword(_hasher.HashPassword(password));

            await _repo.UpdateUserbyIdAsync(existingUser);

            return "User updated successfully";
        }


        public bool VerifyPassword(AppUser user, string password)
        {
            return _hasher.VerifyHashedPassword(user.PasswordHash, password) == PasswordVerificationResult.Success;
        }


    }
}
