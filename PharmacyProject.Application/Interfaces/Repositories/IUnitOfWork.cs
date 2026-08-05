using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<City> Cities { get; }
        IGenericRepository<District> Districts { get; }
        IGenericRepository<Pharmacy> Pharmacies { get; }
        IGenericRepository<InsuranceCompany> InsuranceCompanies { get; }
        IGenericRepository<PharmacyInsurance> PharmacyInsurances { get; }
        IGenericRepository<UnmatchedPharmacy> UnmatchedPharmacies { get; }

        // Use this method to save changes to the database
        Task<int> SaveChangesAsync();

        // Manage transactions
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

    }
}
