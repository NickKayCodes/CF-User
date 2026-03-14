using CF_User.Data;
using CF_User.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace CF_User.Repo.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
            => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<AppUser?> GetByIdAsync(Guid id)
            => await _db.Users.FindAsync(id);

        public async Task AddUserAsync(AppUser user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        public async Task<AppUser> UpdateUserbyIdAsync(AppUser user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(AppUser user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

    }
}
