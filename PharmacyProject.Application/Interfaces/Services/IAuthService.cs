using PharmacyProject.Application.DTOs.Auth;

namespace PharmacyProject.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto request);
        Task<TokenDto> LoginAsync(LoginDto request);
    }
}