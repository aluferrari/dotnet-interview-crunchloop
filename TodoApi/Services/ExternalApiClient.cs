using System.Text;
using System.Text.Json;
using TodoApi.Dtos.External.TodoLists;
using TodoApi.Dtos.External.TodoItems;
using TodoApi.Interfaces;

namespace TodoApi.Services;

public class ExternalApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;

    public ExternalApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ExternalTodoListDto>> ListTodoListsAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("todolists", ct);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<List<ExternalTodoListDto>>(content)!;
    }

    public async Task<string> CreateTodoListAsync(ExternalCreateTodoListDto dto, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(dto);
        var response = await _httpClient.PostAsync(
            "todolists",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct
        );
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<ExternalTodoListDto>(content)!;
        return result.Id; // external ID is string now
    }

    public async Task UpdateTodoListAsync(string externalId, ExternalUpdateTodoListDto dto, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(dto);
        var response = await _httpClient.PatchAsync(
            $"todolists/{externalId}",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTodoListAsync(string externalId, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"todolists/{externalId}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTodoItemAsync(string listExternalId, string itemExternalId, ExternalUpdateTodoItemDto dto, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(dto);
        var response = await _httpClient.PatchAsync(
            $"todolists/{listExternalId}/todoitems/{itemExternalId}",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTodoItemAsync(string listExternalId, string itemExternalId, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"todolists/{listExternalId}/todoitems/{itemExternalId}", ct);
        response.EnsureSuccessStatusCode();
    }
}
