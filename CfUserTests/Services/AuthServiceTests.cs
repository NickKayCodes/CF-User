using CF_User.Data;
using CF_User.Model;
using CF_User.Model.Auth;
using CF_User.Model.enums;
using CF_User.Model.JE;
using CF_User.Services;
using CF_User.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CfUserTests.Services
{
    public class AuthServiceTests
    {
        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private AuthService CreateAuthService(
            AppDbContext db,
            out Mock<ILogger<AuthService>> loggerMock
        )
        {
            loggerMock = new Mock<ILogger<AuthService>>();

            var jwtSettings = new JwtSettings
            {
                SecretKey = "YourSuperSecretKeyThatMustBeLongEnoughForJwtUse1234567890",
                Issuer = "CF-User",
                Audience = "CF-User-API",
            };

            return new AuthService(db, Options.Create(jwtSettings), loggerMock.Object);
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
        {
            using var db = CreateDb();

            var username = "testuser";
            var password = "ValidPassword123!";

            var user = new AppUser(username, "test@example.com", password)
            {
                Role = UserRole.MANAGER,
            };
            user.Privileges.Add(new UserPrivilege { Privilege = Privilege.VIEW_EVENT });

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync(username, password);

            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(UserRole.MANAGER.ToString(), result.Role);
            Assert.Contains(Privilege.VIEW_EVENT.ToString(), result.Privileges);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task LoginAsync_WithNonexistentUser_ReturnsNull()
        {
            using var db = CreateDb();
            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync("ghost", "Password123!");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
        {
            using var db = CreateDb();

            var username = "testuser";
            var correctPassword = "ValidPassword123!";
            var wrongPassword = "WrongPassword123!";

            var user = new AppUser(username, "test@example.com", correctPassword)
            {
                Role = UserRole.SERVER,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync(username, wrongPassword);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithEmptyUsername_ReturnsNull()
        {
            using var db = CreateDb();
            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync("", "Password123!");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithEmptyPassword_ReturnsNull()
        {
            using var db = CreateDb();

            var username = "testuser";
            var user = new AppUser(username, "test@example.com", "ValidPassword123!")
            {
                Role = UserRole.KITCHEN_CHEF,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync(username, "");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithUserWithNoPrivileges_ReturnsEmptyPrivilegesList()
        {
            using var db = CreateDb();

            var username = "testuser";
            var password = "ValidPassword123!";

            var user = new AppUser(username, "test@example.com", password)
            {
                Role = UserRole.WAREHOUSE,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync(username, password);

            Assert.NotNull(result);
            Assert.Empty(result.Privileges);
        }

        [Fact]
        public async Task LoginAsync_WithMultiplePrivileges_ReturnsAllPrivileges()
        {
            using var db = CreateDb();

            var username = "adminuser";
            var password = "AdminPassword123!";

            var user = new AppUser(username, "admin@example.com", password)
            {
                Role = UserRole.ADMIN,
            };

            user.Privileges.Add(new UserPrivilege { Privilege = Privilege.FULL_ACCESS });
            user.Privileges.Add(new UserPrivilege { Privilege = Privilege.VIEW_EVENT });
            user.Privileges.Add(new UserPrivilege { Privilege = Privilege.EDIT_EVENT });

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync(username, password);

            Assert.NotNull(result);
            Assert.Equal(3, result.Privileges.Count());
        }

        [Fact]
        public async Task LoginAsync_GeneratedToken_ContainsCorrectClaims()
        {
            using var db = CreateDb();

            var username = "testuser";
            var password = "ValidPassword123!";

            var user = new AppUser(username, "test@example.com", password)
            {
                Role = UserRole.EVENT_CHEF,
            };

            user.Privileges.Add(new UserPrivilege { Privilege = Privilege.VIEW_MENU });

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out _);

            var result = await auth.LoginAsync(username, password);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);

            var parts = result.Token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public async Task LoginAsync_LogsWarningWhenUserNotFound()
        {
            using var db = CreateDb();
            var auth = CreateAuthService(db, out var loggerMock);

            var result = await auth.LoginAsync("ghost", "Password123!");

            Assert.Null(result);

            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task LoginAsync_LogsWarningWhenPasswordInvalid()
        {
            using var db = CreateDb();

            var username = "testuser";
            var correctPassword = "ValidPassword123!";
            var wrongPassword = "WrongPassword123!";

            var user = new AppUser(username, "test@example.com", correctPassword)
            {
                Role = UserRole.CAPTAIN,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out var loggerMock);

            var result = await auth.LoginAsync(username, wrongPassword);

            Assert.Null(result);

            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid password")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task LoginAsync_LogsSuccessfulLogin()
        {
            using var db = CreateDb();

            var username = "testuser";
            var password = "ValidPassword123!";

            var user = new AppUser(username, "test@example.com", password)
            {
                Role = UserRole.SALES_PLANNER,
            };

            user.Privileges.Add(new UserPrivilege { Privilege = Privilege.VIEW_REPORT });

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var auth = CreateAuthService(db, out var loggerMock);

            var result = await auth.LoginAsync(username, password);

            Assert.NotNull(result);

            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login successful")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ),
                Times.Once
            );
        }

        #endregion
    }
}
