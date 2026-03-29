using System.Data;
using CF_User.Model;
using CF_User.Model.enums;
using CF_User.Model.JE;
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

        /**
         * when Creating a user, Roles needs to be assigned
         * The roles c roles can already have privileges assigned to them,
         * so when a user is created with a role, they automatically inherit the privileges of that role.
         *
         */
        public async Task<AppUser> CreateUserAsync(
            string username,
            string email,
            string password,
            UserRole roles
        )
        {
            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null)
                throw new Exception("Email already in use");

            var hash = _hasher.HashPassword(password);

            var user = new AppUser(username, email, hash) { Role = roles};

            // assign privileges from role
            foreach (var priv in RolePrivilegeMap.Privileges[roles])
            {
                user.Privileges.Add(new UserPrivilege { UserId = user.Id, Privilege = priv });
            }

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

        public async Task<string> UpdateUserByIdAsync(
            Guid id,
            string? username,
            string? email,
            string? password,
            UserRole? role,
            List<Privilege>? privileges
        )
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found");

            // identity fields
            if (username != null)
                user.SetUsername(username);
            if (email != null)
                user.SetEmail(email);
            if (password != null)
                user.SetPassword(_hasher.HashPassword(password));

            // role changed
            if (role != null)
            {
                user.Role = (UserRole)role;

                // if privileges not explicitly provided, recalc from role
                if (privileges == null)
                {
                    user.Privileges.Clear();
                    foreach (var priv in RolePrivilegeMap.Privileges[role.Value])
                    {
                        user.Privileges.Add(
                            new UserPrivilege { UserId = user.Id, Privilege = priv }
                        );
                    }
                }
            }

            // explicit privilege override
            if (privileges != null)
            {
                user.Privileges.Clear();
                foreach (var priv in privileges)
                {
                    user.Privileges.Add(
                        new UserPrivilege { UserId = user.Id, Privilege = priv }
                    );
                }
            }

            await _repo.UpdateUserbyIdAsync(user);
            return "User updated successfully";
        }

        public bool VerifyPassword(AppUser user, string password)
        {
            return _hasher.VerifyHashedPassword(user.PasswordHash, password)
                == PasswordVerificationResult.Success;
        }
    }
}
