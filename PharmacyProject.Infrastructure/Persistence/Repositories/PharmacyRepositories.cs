using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class PharmacyRepository : GenericRepository<Pharmacy>, IPharmacyRepository
    {
        public PharmacyRepository(AppDbContext context) : base(context)
        {
        }

    }
}