using Microsoft.Extensions.Logging;

namespace PharmacyProject.Infrastructure.Workers
{
    public class OnDutyPharmacySyncWorker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OnDutyPharmacySyncWorker> _logger;

        public OnDutyPharmacySyncWorker(IServiceProvider serviceProvider, ILogger<OnDutyPharmacySyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
    }
}