using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class InsuranceRepository : GenericRepository<InsuranceCompany>, IInsuranceRepository
    {
        public InsuranceRepository(AppDbContext context) : base(context)
        {
        }
    }
}