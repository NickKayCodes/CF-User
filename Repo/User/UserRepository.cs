using System;
using CF_User.Data;
using CF_User.Model;
using Microsoft.EntityFrameworkCore;

namespace CF_User.Repo.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext db, ILogger<UserRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Fetching user by email: {Email}", email);

            try
            {
                var normalized = email?.Trim().ToLower();
                var user = await _db.Users
                    .Include(u => u.Privileges)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);

                if (user == null)
                {
                    _logger.LogInformation("User not found for email: {Email}", email);
                    return null;
                }

                _logger.LogInformation("User found for email: {Email} with ID: {UserId}", email, user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email: {Email}. Exception: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task<AppUser?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching user by ID: {UserId}", id);

            try
            {
                var user = await _db.Users
                    .Include(u => u.Privileges)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogInformation("User not found for ID: {UserId}", id);
                    return null;
                }

                _logger.LogInformation("User found for ID: {UserId} with email: {Email}", id, user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by ID: {UserId}. Exception: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task AddUserAsync(AppUser user)
        {
            _logger.LogInformation("Adding new user with email: {Email}, username: {Username}", user.Email, user.Username);

            try
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("User successfully added with ID: {UserId}, email: {Email}", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user with email: {Email}. Exception: {Message}", user.Email, ex.Message);
                throw;
            }
        }

        public async Task<AppUser> UpdateUserbyIdAsync(AppUser user)
        {
            _logger.LogInformation("Updating user with ID: {UserId}, email: {Email}", user.Id, user.Email);

            try
            {
                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("User successfully updated with ID: {UserId}, email: {Email}", user.Id, user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}. Exception: {Message}", user.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteUserAsync(AppUser user)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}, email: {Email}", user.Id, user.Email);

            try
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("User successfully deleted with ID: {UserId}, email: {Email}", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}. Exception: {Message}", user.Id, ex.Message);
                throw;
            }
        }
    }
}
