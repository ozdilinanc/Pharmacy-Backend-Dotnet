namespace PharmacyProject.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IUserRepository Users { get; }
        ICityRepository Cities { get; }
        IDistrictRepository Districts { get; }
        IPharmacyRepository Pharmacies { get; }
        IInsuranceRepository InsuranceCompanies { get; }
        IPharmacyInsuranceRepository PharmacyInsurances { get; }
        IUnmatchedPharmacyRepository UnmatchedPharmacies { get; }

        // Use this method to save changes to the database
        Task<int> SaveChangesAsync();

        // Manage transactions
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

    }
}
