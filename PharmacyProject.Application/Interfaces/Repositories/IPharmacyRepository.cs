using PharmacyProject.Core.Entities;
using System.Linq.Expressions;

namespace PharmacyProject.Application.Interfaces.Repositories
{
    public interface IPharmacyRepository : IGenericRepository<Pharmacy>
    {
        Task<(IEnumerable<Pharmacy> Pharmacies, int TotalCount)> GetPharmaciesWithDetailsAsync(Expression<Func<Pharmacy, bool>>? predicate = null, int pageNumber = 1, int pageSize = 50);
    }
}
