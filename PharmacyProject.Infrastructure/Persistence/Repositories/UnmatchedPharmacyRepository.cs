using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class UnmatchedPharmacyRepository : GenericRepository<UnmatchedPharmacy>, IUnmatchedPharmacyRepository
    {
        public UnmatchedPharmacyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<UnmatchedPharmacy> UnmatchedPharmacies, int TotalCount)> GetUnmatchedWithDetailsAsync(System.Linq.Expressions.Expression<Func<UnmatchedPharmacy, bool>>? predicate = null, int pageNumber = 1, int pageSize = 50)
        {
            var query = _dbSet.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            int totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);

            var unmatched = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                query.OrderBy(u => u.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize));

            return (unmatched, totalCount);
        }
    }
}