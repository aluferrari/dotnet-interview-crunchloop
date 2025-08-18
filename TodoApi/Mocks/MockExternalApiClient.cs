using TodoApi.Dtos.External.TodoLists;
using TodoApi.Dtos.External.TodoItems;
using TodoApi.Interfaces;

namespace TodoApi.Services;

public class MockExternalApiClient : IExternalApiClient
{
    private readonly List<ExternalTodoListDto> _lists;

    public MockExternalApiClient()
    {
        _lists = new List<ExternalTodoListDto>();

        for (int i = 1; i <= 10; i++)
        {
            var list = new ExternalTodoListDto
            {
                Id = $"ext-{i}",
                SourceId = $"local-{i}",
                Name = $"Mock List {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow,
                TodoItems = new List<ExternalTodoItemDto>()
            };

            var itemCount = new Random().Next(3, 6); // 3 to 5 items
            for (int j = 1; j <= itemCount; j++)
            {
                list.TodoItems.Add(new ExternalTodoItemDto
                {
                    Id = $"item-{i}-{j}",
                    SourceId = $"local-item-{i}-{j}",
                    Description = $"Mock item {j} of list {i}",
                    Completed = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-i).AddHours(j),
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _lists.Add(list);
        }
    }

    public Task<List<ExternalTodoListDto>> ListTodoListsAsync(CancellationToken ct = default)
        => Task.FromResult(_lists);

    public Task<string> CreateTodoListAsync(ExternalCreateTodoListDto dto, CancellationToken ct = default)
    {
        var newList = new ExternalTodoListDto
        {
            Id = Guid.NewGuid().ToString(),
            SourceId = dto.SourceId,
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TodoItems = dto.Items?.Select(i => new ExternalTodoItemDto
            {
                Id = Guid.NewGuid().ToString(),
                SourceId = i.SourceId,
                Description = i.Description,
                Completed = i.Completed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList() ?? new List<ExternalTodoItemDto>()
        };

        _lists.Add(newList);
        return Task.FromResult(newList.Id);
    }

    public Task UpdateTodoListAsync(string externalId, ExternalUpdateTodoListDto dto, CancellationToken ct = default)
    {
        var list = _lists.FirstOrDefault(l => l.Id == externalId);
        if (list != null && dto.Name != null)
        {
            list.Name = dto.Name;
            list.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeleteTodoListAsync(string externalId, CancellationToken ct = default)
    {
        var list = _lists.FirstOrDefault(l => l.Id == externalId);
        if (list != null) _lists.Remove(list);
        return Task.CompletedTask;
    }

    public Task UpdateTodoItemAsync(string listExternalId, string itemExternalId, ExternalUpdateTodoItemDto dto, CancellationToken ct = default)
    {
        var list = _lists.FirstOrDefault(l => l.Id == listExternalId);
        var item = list?.TodoItems.FirstOrDefault(i => i.Id == itemExternalId);
        if (item != null)
        {
            if (dto.Description != null) item.Description = dto.Description;
            if (dto.Completed.HasValue) item.Completed = dto.Completed.Value;
            item.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeleteTodoItemAsync(string listExternalId, string itemExternalId, CancellationToken ct = default)
    {
        var list = _lists.FirstOrDefault(l => l.Id == listExternalId);
        var item = list?.TodoItems.FirstOrDefault(i => i.Id == itemExternalId);
        if (item != null) list.TodoItems.Remove(item);
        return Task.CompletedTask;
    }
}
