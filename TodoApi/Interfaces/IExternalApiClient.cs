using TodoApi.Dtos.External.TodoLists;
using TodoApi.Dtos.External.TodoItems;

namespace TodoApi.Interfaces;

public interface IExternalApiClient
{
    // TodoLists
    Task<List<ExternalTodoListDto>> ListTodoListsAsync(CancellationToken ct = default);
    Task<string> CreateTodoListAsync(ExternalCreateTodoListDto dto, CancellationToken ct = default);
    Task UpdateTodoListAsync(string externalId, ExternalUpdateTodoListDto dto, CancellationToken ct = default);
    Task DeleteTodoListAsync(string externalId, CancellationToken ct = default);

    // TodoItems
    Task UpdateTodoItemAsync(string listExternalId, string itemExternalId, ExternalUpdateTodoItemDto dto, CancellationToken ct = default);
    Task DeleteTodoItemAsync(string listExternalId, string itemExternalId, CancellationToken ct = default);
}
