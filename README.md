# PharmacyProject Backend 🚀

Bu proje, çeşitli sigorta şirketlerinin anlaşmalı kurum verilerini (JSON üzerinden) ve il/ilçe bazlı nöbetçi eczane listelerini (API üzerinden) toplayıp birleştiren bir aracı (aggregator) servistir. Ana amacı, farklı kaynaklardan toplanan bu dağınık verileri kendi veritabanımızda eşleştirip, mobil uygulamamıza temiz ve tek bir API üzerinden sunmaktır.

## 🛠️ Kullanılan Teknolojiler ve Mimari

Bu proje, sürdürülebilirlik ve kodun test edilebilirliği göz önünde bulundurularak **Onion Architecture (Soğan Mimarisi)** baz alınarak geliştirilmiştir.

### 🔹 Temel Teknolojiler
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Dil:** C# 12
- **Mimari:** Onion Architecture (Core, Application, Infrastructure, Presentation)

### 🔹 Veritabanı ve Veri Erişimi
- **ORM:** Entity Framework Core (EF Core)
- **Sorgulama:** LINQ
- **Veritabanı:** PostgreSQL (Npgsql)
- **Migration:** EF Core Code-First

### 🔹 Performans ve Altyapı
- **Konteynerizasyon:** Docker & Docker Compose (PostgreSQL ve Redis için)
- **Önbellekleme:** Redis (Sık sorgulanan eczane verilerini hızlandırmak için)
- **Arka Plan Görevleri:** Hangfire (Sigorta ve nöbetçi eczane verilerini periyodik olarak güncel tutmak için)

### 🔹 Tasarım Desenleri (Design Patterns)
- **DTO (Data Transfer Object):** Veritabanı modellerini (Entity) doğrudan dışarı açmamak ve API trafiğini optimize etmek için.
- **Repository Pattern:** Veritabanı işlemlerini merkezileştirmek için (`IGenericRepository`)
- **Unit of Work Pattern:** İşlemleri tek bir transaction (işlem) bütünlüğü içinde kaydetmek için (`IUnitOfWork`)
- **Dependency Injection (DI):** Servislerin esnek çalışması için

### 🔹 Güvenlik ve API Standartları
- **Kimlik Doğrulama:** JWT (JSON Web Token) tabanlı yetkilendirme (Planlanıyor)
- **Konfigürasyon Güvenliği:** .NET User Secrets ve Docker ortam değişkenleri (Şifrelerin gizliliği için)
- **Entegrasyonlar:** Farklı JSON/API servislerinden veri toplamak için HttpClient ve entegrasyon servisleri.

---

## 📂 Proje Mimarisi ve Geliştirme Durumu (✅ Bitenler)

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
