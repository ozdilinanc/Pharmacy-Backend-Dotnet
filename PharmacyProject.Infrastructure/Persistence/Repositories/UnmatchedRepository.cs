using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class UnmatchedRepository : GenericRepository<UnmatchedPharmacy>, IUnmatchedPharmacyRepository
    {
        public UnmatchedRepository(AppDbContext context) : base(context)
        {
        }
    }
}