using PharmacyProject.Core.Entities;
using System.Linq.Expressions;

namespace PharmacyProject.Application.Interfaces.Repositories
{
    public interface IPharmacyRepository : IGenericRepository<Pharmacy>
    {
        Task<IEnumerable<Pharmacy>> GetPharmaciesWithDetailsAsync(Expression<Func<Pharmacy, bool>>? predicate = null);
    }
}
