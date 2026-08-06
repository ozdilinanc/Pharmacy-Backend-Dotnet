namespace PharmacyProject.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // BCrypt tuzu (salt) kendi üretir ve şifrenin içine gizler.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            // Gelen düz şifre ile veritabanındaki karmaşık hash'i karşılaştırır.
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}