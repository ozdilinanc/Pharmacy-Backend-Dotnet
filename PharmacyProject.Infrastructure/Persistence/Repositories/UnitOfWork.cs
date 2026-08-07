using Microsoft.EntityFrameworkCore.Storage;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Infrastructure.Persistence.Context;


namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        private IUserRepository? _userRepository;
        private ICityRepository? _cityRepository;
        private IDistrictRepository? _districtRepository;
        private IPharmacyRepository? _pharmacyRepository;
        private IInsuranceRepository? _insuranceCompanyRepository;
        private IPharmacyInsuranceRepository? _pharmacyInsuranceRepository;
        private IUnmatchedPharmacyRepository? _unmatchedPharmacyRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users
            => _userRepository ??= new UserRepository(_context);

        public ICityRepository Cities
            => _cityRepository ??= new CityRepository(_context);

        public IDistrictRepository Districts
            => _districtRepository ??= new DistrictRepository(_context);

        public IPharmacyRepository Pharmacies
           => _pharmacyRepository ??= new PharmacyRepository(_context);

        public IInsuranceRepository InsuranceCompanies
            => _insuranceCompanyRepository ??= new InsuranceRepository(_context);

        public IPharmacyInsuranceRepository PharmacyInsurances
            => _pharmacyInsuranceRepository ??= new PharmacyInsuranceRepository(_context);

        public IUnmatchedPharmacyRepository UnmatchedPharmacies
            => _unmatchedPharmacyRepository ??= new UnmatchedPharmacyRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction?.CommitAsync()!;
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                await _transaction?.RollbackAsync()!;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            if (_context != null)
            {
                await _context.DisposeAsync();
            }
        }
    }
}
