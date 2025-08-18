using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos.TodoItems;
using TodoApi.Models;

namespace TodoApi.Controllers
{

    [Route("api/todolists/{todoListId}/items")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todolists/{todoListId}/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems(long todoListId)
        {
            var items = await _context.TodoItems
                .Where(i => i.TodoListId == todoListId)
                .Select(i => new TodoItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    IsCompleted = i.IsCompleted,
                    TodoListId = i.TodoListId
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/todolists/{todoListId}/items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long todoListId, int id)
        {
            var item = await _context.TodoItems
                .Where(i => i.TodoListId == todoListId && i.Id == id)
                .Select(i => new TodoItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    IsCompleted = i.IsCompleted,
                    TodoListId = i.TodoListId
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();

            return Ok(item);
        }

        // POST: api/todolists/{todoListId}/items
        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(long todoListId, CreateTodoItemDto payload)
        {
            var todoList = await _context.TodoList.FindAsync(todoListId);
            if (todoList == null) return NotFound();

            var todoItem = new TodoItem
            {
                Title = payload.Title,
                IsCompleted = payload.IsCompleted,
                TodoListId = todoListId
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            var result = new TodoItemDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                IsCompleted = todoItem.IsCompleted,
                TodoListId = todoItem.TodoListId
            };

            return CreatedAtAction(nameof(GetTodoItem), new { todoListId, id = result.Id }, result);

        }

        // PUT: api/todolists/{todoListId}/items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(long todoListId, int id, UpdateTodoItemDto payload)
        {
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(i => i.TodoListId == todoListId && i.Id == id);

            if (todoItem == null) return NotFound();

            todoItem.Title = payload.Title;
            todoItem.IsCompleted = payload.IsCompleted;

            await _context.SaveChangesAsync();

            return Ok(new TodoItemDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                IsCompleted = todoItem.IsCompleted,
                TodoListId = todoItem.TodoListId
            });
        }

        // DELETE: api/todolists/{todoListId}/items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long todoListId, int id)
        {
            var todoItem = await _context.TodoItems
                .FirstOrDefaultAsync(i => i.TodoListId == todoListId && i.Id == id);

            if (todoItem == null) return NotFound();

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
