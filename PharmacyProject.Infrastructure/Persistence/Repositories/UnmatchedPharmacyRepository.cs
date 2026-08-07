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
    }
}