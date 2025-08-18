using System.Text.Json.Serialization;

namespace TodoApi.Models;

public class TodoItem
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }
    public string? ExternalId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public long TodoListId { get; set; }

    [JsonIgnore]
    public TodoList TodoList { get; set; } = null!;
}