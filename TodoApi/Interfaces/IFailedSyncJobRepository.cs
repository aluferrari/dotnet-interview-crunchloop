using TodoApi.Models;

namespace TodoApi.Interfaces
{
    public interface IFailedSyncJobRepository
    {
        Task SaveAsync(FailedSyncJob job);
        Task<List<FailedSyncJob>> GetPendingJobsAsync();
        Task DeleteAsync(int id);
    }
}
