namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? ExternalId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<TodoItem> TodoItems { get; set; } = new();
}