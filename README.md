<pre>
PharmacyProject.sln
│
├── 1. Core (Domain) Katmanı
│   ├── Entities/
│   │   ├── City.cs ✅
│   │   ├── District.cs ✅
│   │   ├── Pharmacy.cs ✅
│   │   ├── InsuranceCompany.cs ✅
│   │   ├── PharmacyInsurance.cs ✅
│   │   ├── User.cs ✅
│   │   └── UnmatchedPharmacy.cs ✅
│   └── Enums/
│       ├── InsuranceType.cs ✅
│       └── UserRole.cs ✅
│
├── 2. Application Katmanı
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── RegisterDto.cs
│   │   │   ├── LoginDto.cs
│   │   │   └── TokenDto.cs
│   │   └── Pharmacy/
│   │       ├── PharmacyDto.cs
│   │       ├── ScrapedPharmacyDto.cs
│   │       └── ManualMatchRequestDto.cs
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IGenericRepository.cs ✅
│   │   │   ├── IPharmacyRepository.cs
│   │   │   ├── IUnitOfWork.cs ✅
│   │   │   ├── ICityRepository.cs
│   │   │   ├── IDistrictRepository.cs
│   │   │   ├── IInsuranceRepository.cs
│   │   │   ├── IPharmacyInsuranceRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   └── IUnmatchedPharmacyRepository.cs
│   │   ├── External/
│   │   │   ├── IInsuranceScraperService.cs
│   │   │   └── IOnDutyApiService.cs
│   │   └── Services/
│   │       ├── IAuthService.cs
│   │       └── ICacheService.cs
│   └── Services/
│       ├── PharmacyMatchingService.cs
│       └── AdminManualMatchService.cs
│
├── 3. Infrastructure Katmanı
│   ├── Persistence/
│   │   ├── Contexts/
│   │   │   └── AppDbContext.cs ✅
│   │   └── Repositories/
│   │       ├── GenericRepository.cs ✅
│   │       ├── UnitOfWork.cs ✅
│   │       ├── PharmacyRepository.cs
│   │       ├── CityRepository.cs
│   │       ├── DistrictRepository.cs
│   │       ├── InsuranceRepository.cs
│   │       ├── PharmacyInsuranceRepository.cs
│   │       ├── UserRepository.cs
│   │       └── UnmatchedRepository.cs
│   ├── ExternalServices/
│   │   ├── AllianzScraperService.cs
│   │   ├── [Diğer Servisler]
│   │   └── OnDutyPharmacyApiService.cs
│   ├── Security/
│   │   └── AuthService.cs
│   ├── Caching/
│   │   └── RedisCacheService.cs
│   └── BackgroundJobs/
│       ├── HangfireJobScheduler.cs
│       └── SyncWorkers.cs
│
└── 4. Presentation (API) Katmanı
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── PharmaciesController.cs
    │   └── AdminController.cs
    ├── Program.cs
    └── appsettings.json
</pre>
