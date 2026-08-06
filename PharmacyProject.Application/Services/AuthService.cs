using PharmacyProject.Application.DTOs.Auth;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Security;
using PharmacyProject.Application.Interfaces.Services;
using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<string> RegisterAsync(RegisterDto request)
        {
            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new Exception("Bu email adresi zaten kullanılıyor!");
            }

            var hashedPassword = _passwordHasher.HashPassword(request.Password);

            var newUser = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            return "Kullanıcı başarıyla oluşturuldu.";
        }

        public async Task<TokenDto> LoginAsync(LoginDto request)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new Exception("Email veya şifre hatalı.");
            }

            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new Exception("Email veya şifre hatalı.");
            }

            var token = _tokenService.GenerateToken(user);

            return new TokenDto
            {
                AccessToken = token
            };
        }
    }
}