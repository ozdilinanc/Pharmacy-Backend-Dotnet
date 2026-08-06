using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        public CityRepository(AppDbContext context) : base(context)
        {
        }
    }
}