using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos.External.TodoLists;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.Mappers;

namespace TodoApi.Services;

public class TodoSyncService : BackgroundService, ITodoSyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IExternalApiClient _externalApiClient;
    private readonly ILogger<TodoSyncService> _logger;

    public TodoSyncService(
        IServiceScopeFactory scopeFactory,
        IExternalApiClient externalApiClient,
        ILogger<TodoSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _externalApiClient = externalApiClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncFromExternalAsync(stoppingToken);
                await SyncToExternalAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Todo sync.");
            }
        }
    }

    public async Task SyncFromExternalAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TodoContext>();

        var externalLists = await _externalApiClient.ListTodoListsAsync(ct);

        foreach (var ext in externalLists)
        {
            var local = await context.TodoList
                .Include(l => l.TodoItems)
                .FirstOrDefaultAsync(l => l.ExternalId == ext.Id, ct);

            if (local == null)
            {
                context.TodoList.Add(TodoMapper.MapExternalListToLocal(ext));
            
            }
            else
            {
                // Update existing list
                local.Name = ext.Name ?? local.Name;
                local.UpdatedAtUtc = ext.UpdatedAt;

                foreach (var extItem in ext.TodoItems)
                {
                    var localItem = local.TodoItems.FirstOrDefault(i => i.ExternalId == extItem.Id);
                    if (localItem == null)
                        local.TodoItems.Add(new TodoItem
                        {
                            Title = extItem.Description ?? "Untitled Item",
                            IsCompleted = extItem.Completed,
                            ExternalId = extItem.Id,
                            UpdatedAtUtc = extItem.UpdatedAt
                        });
                    else
                    {
                        localItem.Title = extItem.Description ?? localItem.Title;
                        localItem.IsCompleted = extItem.Completed;
                        localItem.UpdatedAtUtc = extItem.UpdatedAt;
                    }
                }
            }
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task SyncToExternalAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TodoContext>();

        var listsToSync = await context.TodoList
            .Include(l => l.TodoItems)
            .Where(l => string.IsNullOrEmpty(l.ExternalId) || l.UpdatedAtUtc > DateTime.UtcNow.AddMinutes(-5))
            .ToListAsync(ct);

        foreach (var list in listsToSync)
        {
            try
            {
                if (string.IsNullOrEmpty(list.ExternalId))
                {
                    // Create new list in external API
                    var createDto = TodoMapper.MapLocalListToCreateDto(list);
                    var externalId = await _externalApiClient.CreateTodoListAsync(createDto, ct);
                    list.ExternalId = externalId;
                }
                else
                {
                    // Update existing list
                    var updateDto = TodoMapper.MapLocalListToUpdateDto(list);
                    await _externalApiClient.UpdateTodoListAsync(list.ExternalId, updateDto, ct);

                    // Delete items marked as deleted
                    foreach (var item in list.TodoItems.Where(i => i.IsDeleted && !string.IsNullOrEmpty(i.ExternalId)))
                    {
                        await _externalApiClient.DeleteTodoItemAsync(list.ExternalId, item.ExternalId!, ct);
                    }
                }

                list.UpdatedAtUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync list {ListId}", list.Id);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
