using TodoApi.Dtos.External.TodoLists;
using TodoApi.Dtos.External.TodoItems;
using TodoApi.Models;

namespace TodoApi.Mappers;

public static class TodoMapper
{
    public static TodoList MapExternalListToLocal(ExternalTodoListDto ext)
    {
        return new TodoList
        {
            Name = ext.Name ?? "Unnamed List",
            ExternalId = ext.Id,
            UpdatedAtUtc = ext.UpdatedAt,
            TodoItems = ext.TodoItems?.Select(MapExternalItemToLocal).ToList() ?? new List<TodoItem>()
        };
    }

    public static TodoItem MapExternalItemToLocal(ExternalTodoItemDto ext)
    {
        return new TodoItem
        {
            Title = ext.Description ?? "Untitled Item",
            IsCompleted = ext.Completed,
            ExternalId = ext.Id,
            UpdatedAtUtc = ext.UpdatedAt
        };
    }

    public static ExternalCreateTodoListDto MapLocalListToCreateDto(TodoList list)
    {
        string sourceId = ""; //pull from config file,
        return new ExternalCreateTodoListDto
        {
            Name = list.Name,
            SourceId = sourceId,
            Items = list.TodoItems.Select(i => new ExternalCreateTodoItemDto { Description = i.Title, Completed = i.IsCompleted, SourceId = sourceId }).ToList()
        };
    }

    public static ExternalUpdateTodoListDto MapLocalListToUpdateDto(TodoList list)
    {
        return new ExternalUpdateTodoListDto
        {
            Name = list.Name
        };
    }
}
