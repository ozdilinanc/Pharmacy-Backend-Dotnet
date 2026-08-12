using Hangfire;
using PharmacyProject.Infrastructure.Workers;

namespace PharmacyProject.Infrastructure.BackgroundJobs;

public static class HangfireJobScheduler
{
    public static void ScheduleRecurringJobs()
    {
        // 1. Nöbetçi Eczane Her gece 00:05
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

        //TO-DO SCRAPPER INSURANCES
    }
}