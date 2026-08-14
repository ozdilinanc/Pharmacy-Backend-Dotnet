# PharmacyProject Backend 🚀

Bu proje, çeşitli sigorta şirketlerinin anlaşmalı kurum verilerini (Web Scraping üzerinden) ve il/ilçe bazlı nöbetçi eczane listelerini (NosyAPI üzerinden) toplayıp birleştiren bir aracı (aggregator) servistir. Ana amacı, farklı kaynaklardan toplanan bu dağınık ve standart dışı verileri, **Akıllı Eşleştirme Motoru (Smart Matching Engine)** yardımıyla kendi veritabanımızda tutarlı bir şekilde eşleştirip (telefon, isim benzerliği ve koordinat bazlı), mobil uygulamamıza temiz ve tek bir API üzerinden sunmaktır.

## 🛠️ Kullanılan Teknolojiler ve Mimari

Bu proje, sürdürülebilirlik, yüksek performans ve kodun test edilebilirliği göz önünde bulundurularak **Onion Architecture (Soğan Mimarisi)** baz alınarak geliştirilmiştir.

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
- **Arka Plan Görevleri (Background Jobs):** Hangfire (Sigorta ve nöbetçi eczane verilerini, API Rate Limit'lerini koruyarak asenkron ve periyodik olarak güncellemek için)
- **Konteynerizasyon:** Docker & Docker Compose (PostgreSQL ve Redis için)
- **Önbellekleme:** Redis (Sık sorgulanan eczane verilerini hızlandırmak için - Planlanıyor)
- **Veri İşleme (Batching/Chunking):** On binlerce eczane verisini ve API sonucunu RAM'i şişirmeden (Memory Leak koruması) paketler (Chunk) halinde işleyen bellek dostu mimari.

### 🔹 Tasarım Desenleri (Design Patterns)
- **Repository Pattern:** Veritabanı işlemlerini merkezileştirmek ve soyutlamak için (`IGenericRepository`)
- **Unit of Work Pattern:** Batch işlemlerini tek bir transaction (işlem) bütünlüğü içinde güvenle kaydetmek için (`IUnitOfWork`)
- **DTO (Data Transfer Object):** Veritabanı modellerini doğrudan dışarı açmamak ve API trafiğini optimize etmek için.
- **Dependency Injection (DI):** Servislerin esnek, bağımsız ve test edilebilir şekilde çalışması için.

### 🔹 Güvenlik, Motor ve API Standartları
- **Akıllı Eşleştirme Motoru:** Metin benzerliği (Levenshtein Distance vs.), Geo-Coordinate (Enlem/Boylam mesafe hesaplama) ve normalize edilmiş telefon numaraları üzerinden dağınık verileri eşleştirme yeteneği.
- **Hata Yönetimi (Resilience & Fallback):** Eşleşmeyen verilerin kaybolmaması için Karantina (`UnmatchedPharmacy`) tablosu ve merkezi `GlobalExceptionMiddleware`.
- **Kimlik Doğrulama:** JWT (JSON Web Token) tabanlı yetkilendirme (Planlanıyor)
- **Konfigürasyon Güvenliği:** .NET User Secrets ve Docker ortam değişkenleri (API anahtarları ve şifrelerin gizliliği için)
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
│   ├── Enums/
│   │   ├── InsuranceCompanyEnum.cs ✅
│   │   └── UserRole.cs ✅
│   └── DependencyInjection.cs
│
├── 2. Application Katmanı
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── RegisterDto.cs ✅
│   │   │   ├── LoginDto.cs ✅
│   │   │   └── TokenDto.cs ✅
│   │   ├── External/
│   │   │   ├── ScrapedPharmacyDto.cs ✅
│   │   │   └── NosyApi/
│   │   │       ├── NosyApiRecentPharmacyDto.cs ✅
│   │   │       └── NosyApiOnDutyPharmacyDto.cs ✅
│   │   └── Pharmacy/
│   │       ├── CreatePharmacyDto.cs ✅
│   │       ├── PharmacyResponseDto.cs ✅
│   │       ├── UpdatePharmacyDto.cs ✅
│   │       └── ManualMatchRequestDto.cs ✅
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IGenericRepository.cs ✅
│   │   │   ├── IPharmacyRepository.cs ✅
│   │   │   ├── IUnitOfWork.cs ✅
│   │   │   ├── ICityRepository.cs ✅
│   │   │   ├── IDistrictRepository.cs ✅
│   │   │   ├── IInsuranceRepository.cs ✅
│   │   │   ├── IPharmacyInsuranceRepository.cs ✅
│   │   │   ├── IUserRepository.cs ✅
│   │   │   └── IUnmatchedPharmacyRepository.cs ✅
│   │   ├── Security/
│   │   │   ├── ITokenService.cs ✅
│   │   │   └── IPasswordHasher.cs ✅
│   │   ├── External/
│   │   │   ├── IInsuranceScraperService.cs ✅
│   │   │   └── INoisyApiService.cs ✅
│   │   └── Services/
│   │       ├── IAuthService.cs ✅
│   │       ├── IPharmacyService.cs ✅
│   │       └── ICacheService.cs
│   ├── Services/
│   │   ├── PharmacyMatchingService.cs ✅
│   │   ├── AuthService.cs ✅
│   │   ├── PharmacyService.cs ✅
│   │   └── AdminManualMatchService.cs ✅
│   ├── Helpers/
│   │   ├── TextHelper.cs ✅
│   │   └── GeoHelper.cs ✅
│   └── DependencyInjection.cs
│
├── 3. Infrastructure Katmanı
│   ├── Persistence/
│   │   ├── DatabaseSeeder.cs ✅
│   │   ├── Contexts/
│   │   │   └── AppDbContext.cs ✅
│   │   └── Repositories/
│   │       ├── GenericRepository.cs ✅
│   │       ├── UnitOfWork.cs ✅
│   │       ├── PharmacyRepository.cs ✅
│   │       ├── CityRepository.cs ✅
│   │       ├── DistrictRepository.cs ✅
│   │       ├── InsuranceRepository.cs ✅
│   │       ├── PharmacyInsuranceRepository.cs ✅
│   │       ├── UserRepository.cs ✅
│   │       └── UnmatchedPharmacyRepository.cs ✅
│   ├── ExternalServices/
│   │   ├── NoisyApiService.cs ✅
│   │   └── ScraperServices/
│   │       ├── AksigortaScraperService.cs ✅
│   │       ├── AllianzScraperService.cs ✅
│   │       ├── AnadoluSigortaScraperService.cs ✅
│   │       ├── AxaSigortaScraperService.cs ✅
│   │       ├── BupaAcibademSigortaScraperService.cs ✅
│   │       ├── EurekoSigortaScraperService.cs ✅
│   │       ├── MapfreSigortaScraperService.cs ✅
│   │       └── TurkiyeSigortaScraperService.cs ✅
│   ├── Security/
│   │   ├── PasswordHasher.cs ✅
│   │   └── TokenService.cs ✅
│   ├── Workers/
│   │   ├── OnDutyPharmacySyncWorker.cs ✅
│   │   ├── InsuranceSyncWorker.cs ✅
│   │   └── RecentPharmacySyncWorker.cs ✅
│   ├── Caching/
│   │   └── RedisCacheService.cs
│   ├── BackgroundJobs/
│   │   └── HangfireJobScheduler.cs ✅
│   └── DependencyInjection.cs
│
└── 4. Presentation (API) Katmanı
    ├── Controllers/
    │   ├── AuthController.cs ✅
    │   ├── PharmaciesController.cs ✅
    │   └── AdminController.cs ✅
    ├── Middleware/
    │   └── GlobalExceptionMiddleware.cs ✅
    ├── ililce.json
    ├── pharmacies_seed.json
    ├── Program.cs
    ├── DependencyInjection.cs
    └── appsettings.json
</pre>
