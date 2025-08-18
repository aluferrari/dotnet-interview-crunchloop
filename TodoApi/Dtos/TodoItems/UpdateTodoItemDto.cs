namespace TodoApi.Dtos.TodoItems
{
    public class UpdateTodoItemDto
    {
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
    }
}
