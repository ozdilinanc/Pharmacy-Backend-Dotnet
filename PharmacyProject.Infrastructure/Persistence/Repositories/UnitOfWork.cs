using Microsoft.EntityFrameworkCore.Storage;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;


namespace PharmacyProject.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        private IGenericRepository<User>? _userRepository;
        private IGenericRepository<City>? _cityRepository;
        private IGenericRepository<District>? _districtRepository;
        private IGenericRepository<Pharmacy>? _pharmacyRepository;
        private IGenericRepository<InsuranceCompany>? _insuranceCompanyRepository;
        private IGenericRepository<PharmacyInsurance>? _pharmacyInsuranceRepository;
        private IGenericRepository<UnmatchedPharmacy>? _unmatchedPharmacyRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<User> Users
            => _userRepository ??= new GenericRepository<User>(_context);

        public IGenericRepository<City> Cities
            => _cityRepository ??= new GenericRepository<City>(_context);

        public IGenericRepository<District> Districts
            => _districtRepository ??= new GenericRepository<District>(_context);

        public IGenericRepository<Pharmacy> Pharmacies
           => _pharmacyRepository ??= new GenericRepository<Pharmacy>(_context);

        public IGenericRepository<InsuranceCompany> InsuranceCompanies
            => _insuranceCompanyRepository ??= new GenericRepository<InsuranceCompany>(_context);

        public IGenericRepository<PharmacyInsurance> PharmacyInsurances
            => _pharmacyInsuranceRepository ??= new GenericRepository<PharmacyInsurance>(_context);

        public IGenericRepository<UnmatchedPharmacy> UnmatchedPharmacies
            => _unmatchedPharmacyRepository ??= new GenericRepository<UnmatchedPharmacy>(_context);


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
