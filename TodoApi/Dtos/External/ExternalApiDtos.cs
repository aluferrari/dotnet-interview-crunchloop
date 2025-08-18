using TodoApi.Dtos.External.TodoItems;

namespace TodoApi.Dtos.External.TodoLists
{
    public class ExternalTodoListDto
    {
        public string Id { get; set; } = default!;
        public string SourceId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ExternalTodoItemDto> TodoItems { get; set; } = new();
    }

    public class ExternalCreateTodoListDto
    {
        public string SourceId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<ExternalCreateTodoItemDto>? Items { get; set; }
    }

    public class ExternalUpdateTodoListDto
    {
        public string Name { get; set; } = default!;
    }
}

namespace TodoApi.Dtos.External.TodoItems
{
    public class ExternalTodoItemDto
    {
        public string Id { get; set; } = default!;
        public string SourceId { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool Completed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ExternalCreateTodoItemDto
    {
        public string SourceId { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool Completed { get; set; }
    }

    public class ExternalUpdateTodoItemDto
    {
        public string? Description { get; set; }
        public bool? Completed { get; set; }
    }
}
