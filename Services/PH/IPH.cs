using Microsoft.AspNetCore.Identity;

namespace CF_User.Services.PH
{
    public interface IPH
    {
            string HashPassword(string password);
            PasswordVerificationResult VerifyHashedPassword(string passwordHash, string password);
    }
}
