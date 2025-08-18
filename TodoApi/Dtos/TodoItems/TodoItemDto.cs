using System.Text.Json.Serialization;

namespace TodoApi.Dtos.TodoItems
{
    public class TodoItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }

        [JsonIgnore]
        public long TodoListId { get; set; }
    }
}
