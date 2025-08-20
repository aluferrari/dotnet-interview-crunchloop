using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApi.Interfaces;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class FailedSyncJobRepository : IFailedSyncJobRepository
    {
        private readonly TodoContext _db;

        public FailedSyncJobRepository(TodoContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(FailedSyncJob job)
        {
            _db.FailedSyncJobs.Add(job);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FailedSyncJob>> GetPendingJobsAsync()
        {
            return await _db.FailedSyncJobs
                .AsNoTracking()
                .OrderBy(f => f.FailedAt)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var job = await _db.FailedSyncJobs.FindAsync(id);
            if (job != null)
            {
                _db.FailedSyncJobs.Remove(job);
                await _db.SaveChangesAsync();
            }
        }
    }
}
