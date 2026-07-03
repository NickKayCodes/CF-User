using System;
using CF_User.Model;

namespace CF_User.Model.Auth
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TokenHash { get; set; } = string.Empty; // store hashed token
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ReplacedByTokenHash { get; set; }
        public string? RemoteIpAddress { get; set; }
        public string? UserAgent { get; set; }

        // navigation
        public AppUser? User { get; set; }
    }
}
