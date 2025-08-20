using System.Text;
using System.Text.Json;
using TodoApi.Dtos.External.TodoItems;
using TodoApi.Dtos.External.TodoLists;
using TodoApi.Interfaces;
using TodoApi.Models;

namespace TodoApi.Services;

public class ExternalApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IFailedSyncJobRepository _failedRepo;

    public ExternalApiClient(HttpClient httpClient, IFailedSyncJobRepository failedRepo)
    {
        _httpClient = httpClient;
        _failedRepo = failedRepo;
    }

    private async Task<T> HandleRequestAsync<T>(
        Func<Task<HttpResponseMessage>> httpCall,
        string operation,
        string url,
        string? payload = null,
        CancellationToken ct = default)
    {
        try
        {
            var response = await httpCall();
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(content)!;
        }
        catch (Exception ex)
        {
            await _failedRepo.SaveAsync(new FailedSyncJob
            {
                Operation = operation,
                Payload = payload ?? string.Empty,
                ErrorMessage = ex.ToString()
            });

            throw;
        }
    }

    private async Task HandleRequestAsync(
        Func<Task<HttpResponseMessage>> httpCall,
        string operation,
        string url,
        string? payload = null,
        CancellationToken ct = default)
    {
        try
        {
            var response = await httpCall();
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            await _failedRepo.SaveAsync(new FailedSyncJob
            {
                Operation = operation,
                Payload = payload ?? string.Empty,
                ErrorMessage = ex.ToString()
            });

            throw;
        }
    }

    public Task<List<ExternalTodoListDto>> ListTodoListsAsync(CancellationToken ct = default) =>
        HandleRequestAsync<List<ExternalTodoListDto>>(
            () => _httpClient.GetAsync("todolists", ct),
            "ListTodoLists",
            "todolists",
            null,
            ct);

    public Task<string> CreateTodoListAsync(ExternalCreateTodoListDto dto, CancellationToken ct = default) =>
        HandleRequestAsync<ExternalTodoListDto>(
            () => _httpClient.PostAsync(
                "todolists",
                new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"),
                ct),
            "CreateTodoList",
            "todolists",
            JsonSerializer.Serialize(dto),
            ct).ContinueWith(t => t.Result.Id, ct);

    public Task UpdateTodoListAsync(string externalId, ExternalUpdateTodoListDto dto, CancellationToken ct = default) =>
        HandleRequestAsync(
            () => _httpClient.PatchAsync(
                $"todolists/{externalId}",
                new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"),
                ct),
            "UpdateTodoList",
            $"todolists/{externalId}",
            JsonSerializer.Serialize(dto),
            ct);

    public Task DeleteTodoListAsync(string externalId, CancellationToken ct = default) =>
        HandleRequestAsync(
            () => _httpClient.DeleteAsync($"todolists/{externalId}", ct),
            "DeleteTodoList",
            $"todolists/{externalId}",
            null,
            ct);

    public Task UpdateTodoItemAsync(string listExternalId, string itemExternalId, ExternalUpdateTodoItemDto dto, CancellationToken ct = default) =>
        HandleRequestAsync(
            () => _httpClient.PatchAsync(
                $"todolists/{listExternalId}/todoitems/{itemExternalId}",
                new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"),
                ct),
            "UpdateTodoItem",
            $"todolists/{listExternalId}/todoitems/{itemExternalId}",
            JsonSerializer.Serialize(dto),
            ct);

    public Task DeleteTodoItemAsync(string listExternalId, string itemExternalId, CancellationToken ct = default) =>
        HandleRequestAsync(
            () => _httpClient.DeleteAsync($"todolists/{listExternalId}/todoitems/{itemExternalId}", ct),
            "DeleteTodoItem",
            $"todolists/{listExternalId}/todoitems/{itemExternalId}",
            null,
            ct);
}
