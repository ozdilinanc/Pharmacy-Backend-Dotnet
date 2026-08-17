using PharmacyProject.Core.Entities;
using PharmacyProject.Infrastructure.Persistence.Context;
using System.Text.Json;

namespace PharmacyProject.Infrastructure.Persistence
{
    public static class DatabaseSeeder
    {
        public static async Task SeedCitiesAndDistrictsAsync(AppDbContext context)
        {
            if (context.Cities.Any()) return;

            var filepath = Path.Combine(AppContext.BaseDirectory, "ililce.json");
            if (!File.Exists(filepath)) return;

            var jsonData = await File.ReadAllTextAsync(filepath);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var citiesData = JsonSerializer.Deserialize<List<CitySeedModel>>(jsonData, options);

            if (citiesData != null)
            {
                var cities = citiesData.Select(c => new City
                {
                    Name = c.Name,
                    Slug = c.Slug,
                    Districts = c.Districts.Select(d => new District
                    {
                        Name = d.Name,
                        Slug = d.Slug,
                    }).ToList()
                }).ToList();

                await context.Cities.AddRangeAsync(cities);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedPharmaciesAsync(AppDbContext context)
        {
            if (context.Pharmacies.Any()) return;

            var filepath = Path.Combine(AppContext.BaseDirectory, "pharmacies_seed.json");
            if (!File.Exists(filepath)) return;

            var jsonData = await File.ReadAllTextAsync(filepath);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pharmaciesData = JsonSerializer.Deserialize<List<PharmacySeedModel>>(jsonData, options);

            if (pharmaciesData != null && pharmaciesData.Any())
            {
                var pharmacies = pharmaciesData.Select(p => new Pharmacy
                {
                    Name = p.Name,
                    Address = p.Address,
                    PhoneNumber = p.PhoneNumber,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    DistrictId = p.DistrictId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                int batchSize = 5000;
                for (int i = 0; i < pharmacies.Count; i += batchSize)
                {
                    var batch = pharmacies.Skip(i).Take(batchSize).ToList();
                    await context.Pharmacies.AddRangeAsync(batch);
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task SeedInsuranceCompaniesAsync(AppDbContext context)
        {
            if (!context.InsuranceCompanies.Any())
            {
                var defaultInsurances = new List<InsuranceCompany>
                {
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.Aksigorta.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.Allianz.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.AnadoluSigorta.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.AxaSigorta.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.BupaAcibademSigorta.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.EurekoSigorta.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.MapfreSigorta.ToString() },
                    new InsuranceCompany { Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.TurkiyeSigorta.ToString() }
                };

                await context.InsuranceCompanies.AddRangeAsync(defaultInsurances);
                await context.SaveChangesAsync();
            }
            else
            {
                // Mevcut yanlış isimlendirmeleri (Axa, Mapfre vb.) Enum ile uyumlu hale getirmek için düzeltme yaması
                var existingInsurances = context.InsuranceCompanies.ToList();
                bool needsUpdate = false;
                foreach (var ins in existingInsurances)
                {
                    if (ins.Name == "AkSigorta") { ins.Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.Aksigorta.ToString(); needsUpdate = true; }
                    else if (ins.Name == "Axa") { ins.Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.AxaSigorta.ToString(); needsUpdate = true; }
                    else if (ins.Name == "BupaAcibadem") { ins.Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.BupaAcibademSigorta.ToString(); needsUpdate = true; }
                    else if (ins.Name == "Eureko") { ins.Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.EurekoSigorta.ToString(); needsUpdate = true; }
                    else if (ins.Name == "Mapfre") { ins.Name = PharmacyProject.Core.Enums.InsuranceCompanyEnum.MapfreSigorta.ToString(); needsUpdate = true; }
                }

                if (needsUpdate)
                {
                    context.InsuranceCompanies.UpdateRange(existingInsurances);
                    await context.SaveChangesAsync();
                }
            }
        }

        private class CitySeedModel
        {
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public List<DistrictSeedModel> Districts { get; set; } = new();
        }

        public static async Task SeedSuperAdminAsync(AppDbContext context, PharmacyProject.Application.Interfaces.Security.IPasswordHasher passwordHasher, string defaultPassword)
        {
            if (!context.Users.Any(u => u.Role == PharmacyProject.Core.Enums.UserRole.SuperAdmin))
            {
                var superAdmin = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@pharmacy.com",
                    PasswordHash = passwordHasher.HashPassword(defaultPassword),
                    Role = PharmacyProject.Core.Enums.UserRole.SuperAdmin,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(superAdmin);
                await context.SaveChangesAsync();
            }
        }

        private class DistrictSeedModel
        {
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
        }

        private class PharmacySeedModel
        {
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int DistrictId { get; set; }
        }
    }
}