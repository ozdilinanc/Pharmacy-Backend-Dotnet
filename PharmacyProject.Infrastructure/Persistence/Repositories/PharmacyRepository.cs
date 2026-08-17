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

        public async Task<(IEnumerable<Pharmacy> Pharmacies, int TotalCount)> GetPharmaciesWithDetailsAsync(Expression<Func<Pharmacy, bool>>? predicate = null, int pageNumber = 1, int pageSize = 50)
        {
            var query = _dbSet
                .Include(p => p.District).ThenInclude(d => d.City)
                .Include(p => p.PharmacyInsurances).ThenInclude(pi => pi.InsuranceCompany)
                .AsQueryable();
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            int totalCount = await query.CountAsync();
            
            var pharmacies = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (pharmacies, totalCount);
        }
    }
}
