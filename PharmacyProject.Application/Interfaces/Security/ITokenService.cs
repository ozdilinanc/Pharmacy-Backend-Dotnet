using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Interfaces.Security
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}