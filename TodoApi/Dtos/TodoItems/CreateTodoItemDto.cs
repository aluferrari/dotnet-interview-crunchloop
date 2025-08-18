namespace TodoApi.Dtos.TodoItems
{
    public class CreateTodoItemDto
    {
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
    }
}
