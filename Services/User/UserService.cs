using CF_User.Data.TO.Create;
using CF_User.Data.TO.Delete;
using CF_User.Data.TO.Get;
using CF_User.Data.TO.Update;
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
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repo, IPH hasher, ILogger<UserService> logger)
        {
            _repo = repo;
            _hasher = hasher;
            _logger = logger;
        }

        /**
         * when Creating a user, Roles needs to be assigned
         * The roles can already have privileges assigned to them,
         * so when a user is created with a role, they automatically inherit the privileges of that role.
         */
        public async Task<CreateUserResponse> CreateUserAsync(
            string username,
            string email,
            string password,
            UserRole role
        )
        {
            _logger.LogInformation("CreateUserAsync called for email: {Email}, username: {Username}, role: {Role}", email, username, role);

            try
            {
                // normalize email for consistency
                email = email?.Trim().ToLower();
                username = username?.Trim();

                var existing = await _repo.GetByEmailAsync(email);
                if (existing != null)
                {
                    _logger.LogWarning("User creation failed: Email already in use - {Email}", email);
                    throw new Exception("Email already in use");
                }

                _logger.LogInformation("Email {Email} is available, proceeding with user creation", email);

                var hash = _hasher.HashPassword(password);
                // log hash characteristics (do not log full hash in production)
                _logger.LogInformation("Password hashed for new user - hashPrefix: {HashPrefix}, length: {HashLength}",
                    hash?.Substring(0, Math.Min(8, hash.Length)), hash?.Length ?? 0);

                var user = new AppUser();
                user.SetUsername(username);
                user.SetEmail(email);
                user.SetPasswordHash(hash); // already hashed by IPH
                user.Role = role;

                _logger.LogInformation("Assigning privileges for role: {Role}", role);

                // assign privileges from role
                var privilegesList = RolePrivilegeMap.Privileges[role];
                _logger.LogInformation("Role {Role} has {PrivilegeCount} privileges", role, privilegesList.Count);

                foreach (var priv in privilegesList)
                {
                    user.Privileges.Add(new UserPrivilege { UserId = user.Id, Privilege = priv });
                }

                await _repo.AddUserAsync(user);

                _logger.LogInformation("User created successfully - ID: {UserId}, email: {Email}, role: {Role}, privileges assigned: {PrivilegeCount}", 
                    user.Id, user.Email, user.Role, user.Privileges.Count);

                return new CreateUserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}, username: {Username}. Exception: {Message}", 
                    email, username, ex.Message);
                throw;
            }
        }

        public async Task<DeleteUserResponse> DeleteUserByIdAsync(Guid id)
        {
            _logger.LogInformation("DeleteUserByIdAsync called for user ID: {UserId}", id);

            try
            {
                var existingUser = await _repo.GetByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("User deletion failed: User not found - ID: {UserId}", id);
                    throw new Exception("User not found");
                }

                _logger.LogInformation("User found - email: {Email}, username: {Username}, proceeding with deletion", 
                    existingUser.Email, existingUser.Username);

                await _repo.DeleteUserAsync(existingUser);

                _logger.LogInformation("User successfully deleted - ID: {UserId}, email: {Email}", id, existingUser.Email);

                return new DeleteUserResponse
                {
                    Id = id,
                    Message = "User deleted successfully",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}. Exception: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<GetByEmailResponse> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("GetUserByEmailAsync called for email: {Email}", email);

            try
            {
                var existingUser = await _repo.GetByEmailAsync(email);
                if (existingUser == null)
                {
                    _logger.LogWarning("User retrieval failed: User not found for email: {Email}", email);
                    throw new Exception("Email does not exist");
                }

                _logger.LogInformation("User retrieved successfully - ID: {UserId}, email: {Email}", existingUser.Id, existingUser.Email);
                
                return new GetByEmailResponse
                {
                    Id = existingUser.Id,
                    Username = existingUser.Username,
                    Email = existingUser.Email,
                    Role = existingUser.Role,
                    Privileges = existingUser.Privileges.Select(p => p.Privilege).ToList(),
                    CreatedAt = existingUser.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}. Exception: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task<UpdateUserResponse> UpdateUserByIdAsync(
            Guid id,
            string? username,
            string? email,
            string? password,
            UserRole? role,
            List<Privilege>? privileges
        )
        {
            _logger.LogInformation("UpdateUserByIdAsync called for user ID: {UserId}", id);
            _logger.LogInformation("Update parameters - username: {Username}, email: {Email}, role: {Role}, privileges count: {PrivilegeCount}", 
                username ?? "not provided", email ?? "not provided", role?.ToString() ?? "not provided", privileges?.Count ?? 0);

            try
            {
                var user = await _repo.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User update failed: User not found - ID: {UserId}", id);
                    throw new Exception("User not found");
                }

                _logger.LogInformation("User found for update - email: {Email}, current role: {Role}", user.Email, user.Role);

                // identity fields
                if (username != null)
                {
                    _logger.LogInformation("Updating username from {OldUsername} to {NewUsername}", user.Username, username);
                    user.SetUsername(username);
                }

                if (email != null)
                {
                    var normalizedEmail = email.Trim().ToLower();
                    _logger.LogInformation("Updating email from {OldEmail} to {NewEmail}", user.Email, normalizedEmail);
                    user.SetEmail(normalizedEmail);
                }

                if (password != null)
                {
                    _logger.LogInformation("Password update requested for user ID: {UserId}", id);
                    user.SetPasswordHash(_hasher.HashPassword(password));
                }

                // role changed
                if (role != null)
                {
                    _logger.LogInformation("Role change detected for user ID: {UserId} from {OldRole} to {NewRole}", id, user.Role, role);
                    user.Role = (UserRole)role;

                    // if privileges not explicitly provided, recalc from role
                    if (privileges == null)
                    {
                        _logger.LogInformation("No explicit privileges provided, recalculating from new role: {Role}", role);
                        user.Privileges.Clear();

                        foreach (var priv in RolePrivilegeMap.Privileges[role.Value])
                        {
                            user.Privileges.Add(
                                new UserPrivilege { UserId = user.Id, Privilege = priv }
                            );
                        }

                        _logger.LogInformation("Assigned {PrivilegeCount} privileges from role {Role}", user.Privileges.Count, role);
                    }
                }

                // explicit privilege override
                if (privileges != null)
                {
                    _logger.LogInformation("Explicit privilege override for user ID: {UserId}, privilege count: {PrivilegeCount}", id, privileges.Count);
                    user.Privileges.Clear();

                    foreach (var priv in privileges)
                    {
                        user.Privileges.Add(
                            new UserPrivilege { UserId = user.Id, Privilege = priv }
                        );
                    }

                    _logger.LogInformation("Assigned {PrivilegeCount} explicit privileges", user.Privileges.Count);
                }

                await _repo.UpdateUserbyIdAsync(user);

                _logger.LogInformation("User successfully updated - ID: {UserId}, email: {Email}, role: {Role}", id, user.Email, user.Role);

                return new UpdateUserResponse
                {
                    Id = id,
                    Message = "User updated successfully",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}. Exception: {Message}", id, ex.Message);
                throw;
            }
        }

        public bool VerifyPassword(AppUser user, string password)
        {
            _logger.LogInformation("Verifying password for user ID: {UserId}", user.Id);

            try
            {
                var result = _hasher.VerifyHashedPassword(user.PasswordHash, password) == PasswordVerificationResult.Success;
                
                if (result)
                {
                    _logger.LogInformation("Password verification successful for user ID: {UserId}", user.Id);
                }
                else
                {
                    _logger.LogInformation("Password verification failed for user ID: {UserId}", user.Id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user ID: {UserId}. Exception: {Message}", user.Id, ex.Message);
                throw;
            }
        }
    }
}
