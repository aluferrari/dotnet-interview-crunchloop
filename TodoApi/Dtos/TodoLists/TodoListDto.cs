using TodoApi.Dtos.TodoItems;

namespace TodoApi.Dtos.TodoLists
{
    public class TodoListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public IList<TodoItemDto>? TodoItems { get; set; }
    }
}
