namespace Business.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

        public static bool Verify(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}
