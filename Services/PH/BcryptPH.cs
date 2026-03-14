using Microsoft.AspNetCore.Identity;

namespace CF_User.Services.PH
{
    public class BcryptPH : IPH
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string passwordHash, string password)
        {
            bool verified = BCrypt.Net.BCrypt.Verify(password, passwordHash);

            return verified
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

    }
}
