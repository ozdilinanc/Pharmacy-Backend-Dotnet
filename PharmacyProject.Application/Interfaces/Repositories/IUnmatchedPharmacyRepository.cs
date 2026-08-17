using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Interfaces.Repositories
{
    public interface IUnmatchedPharmacyRepository : IGenericRepository<UnmatchedPharmacy>
    {
        Task<(IEnumerable<UnmatchedPharmacy> UnmatchedPharmacies, int TotalCount)> GetUnmatchedWithDetailsAsync(System.Linq.Expressions.Expression<Func<UnmatchedPharmacy, bool>>? predicate = null, int pageNumber = 1, int pageSize = 50);
    }
}