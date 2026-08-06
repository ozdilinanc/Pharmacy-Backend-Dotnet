using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class PharmacyInsuranceRepository : GenericRepository<PharmacyInsurance>, IPharmacyInsuranceRepository
    {
        public PharmacyInsuranceRepository(AppDbContext context) : base(context)
        {
        }
    }
}