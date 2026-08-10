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

        private class CitySeedModel
        {
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public List<DistrictSeedModel> Districts { get; set; } = new();
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