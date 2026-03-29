namespace CF_User.Data.TO.Login
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public IEnumerable<string> Privileges { get; set; } = [];
    }
}
