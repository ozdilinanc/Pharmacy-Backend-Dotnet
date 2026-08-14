using Microsoft.EntityFrameworkCore;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class PharmacyRepository : GenericRepository<Pharmacy>, IPharmacyRepository
    {
        public PharmacyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pharmacy>> GetPharmaciesWithDetailsAsync(Expression<Func<Pharmacy, bool>>? predicate = null)
        {
            var query = _dbSet.Include(p => p.District).ThenInclude(d => d.City).AsQueryable();
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            return await query.ToListAsync();
        }
    }
}
