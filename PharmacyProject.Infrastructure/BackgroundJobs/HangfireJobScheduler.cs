using Hangfire;
using PharmacyProject.Infrastructure.ExternalServices.Scrapers;
using PharmacyProject.Infrastructure.ExternalServices.ScraperServices;
using PharmacyProject.Infrastructure.Workers;

namespace PharmacyProject.Infrastructure.BackgroundJobs;

public static class HangfireJobScheduler
{
    public static void ScheduleRecurringJobs()
    {
        // 1. Nöbetçi Eczane Her Gece 00:05
        RecurringJob.AddOrUpdate<OnDutyPharmacySyncWorker>(
            "nobetci-eczane-guncelle",
            worker => worker.SyncOnDutyPharmaciesAsync(),
            "5 0 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        // 2. Yeni Eczane Haftada 1 gün
        RecurringJob.AddOrUpdate<RecentPharmacySyncWorker>(
            "yeni-eczane-guncelle",
            worker => worker.SyncRecentPharmaciesAsync(),
            "0 0 * * 0",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        // 3. Anlaşmalı Sigortalar Haftada 1 

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "aksigorta-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.Aksigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 1), // Pazar saat 01:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "allianz-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.Allianz.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 2), // Pazar saat 02:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "anadolu-sigorta-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.AnadoluSigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 3), // Pazar saat 03:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "axa-sigorta-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.AxaSigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 4), // Pazar saat 04:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "bupa-acibadem-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.BupaAcibademSigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 5), // Pazar saat 05:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "eureko-sigorta-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.EurekoSigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 6), // Pazar saat 06:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "mapfre-sigorta-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.MapfreSigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 7), // Pazar saat 07:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );

        RecurringJob.AddOrUpdate<InsuranceSyncWorker>(
            "turkiye-sigorta-sync",
            worker => worker.SyncInsuranceAsync(PharmacyProject.Core.Enums.InsuranceCompanyEnum.TurkiyeSigorta.ToString()),
            Cron.Weekly(DayOfWeek.Sunday, 8), // Pazar saat 08:00
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local }
        );
    }
}