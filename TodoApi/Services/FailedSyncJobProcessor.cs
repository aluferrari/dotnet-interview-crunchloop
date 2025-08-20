using TodoApi.Interfaces;
using TodoApi.Repositories;

namespace TodoApi.Services
{
    public class FailedSyncJobProcessor : BackgroundService
    {
        private readonly FailedSyncJobRepository _repo;

        public FailedSyncJobProcessor(FailedSyncJobRepository repo, IExternalApiClient externalApi)
        {
            _repo = repo;
    }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
