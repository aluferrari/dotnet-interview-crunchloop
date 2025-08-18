namespace TodoApi.Interfaces
{
    public interface ITodoSyncService
    {
        Task SyncFromExternalAsync(CancellationToken ct = default);
        Task SyncToExternalAsync(CancellationToken ct = default);
    }
}
