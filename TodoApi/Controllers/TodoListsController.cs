using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos.TodoItems;
using TodoApi.Dtos.TodoLists;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoListsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todolists
        [HttpGet]
        public async Task<ActionResult<IList<TodoList>>> GetTodoLists([FromQuery] bool includeItems)
        {
            var query = _context.TodoList.AsQueryable();

            if (includeItems)
            {
                return Ok(await query
                    .Include(tl => tl.TodoItems)
                    .Select(tl => new TodoListDto
                    {
                        Id = tl.Id,
                        Name = tl.Name,
                        TodoItems = tl.TodoItems.Select(ti => new TodoItemDto
                        {
                            Id = ti.Id,
                            Title = ti.Title
                        }).ToList()
                    })
                    .ToListAsync());
            }
            else
            {
                return Ok(await query
                    .Select(tl => new TodoListDto
                    {
                        Id = tl.Id,
                        Name = tl.Name,
                        TodoItems = null
                    })
                    .ToListAsync());
            }
        }

        [HttpGet("paged")]
        public async Task<ActionResult<object>> GetPagedTodoLists([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeItems = false)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.TodoList.AsQueryable();

            if (includeItems)
                query = query.Include(tl => tl.TodoItems);

            var totalCount = await query.CountAsync();

            var lists = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(tl => new TodoListDto
                {
                    Id = tl.Id,
                    Name = tl.Name,
                    TodoItems = includeItems
                        ? tl.TodoItems.Select(ti => new TodoItemDto
                        {
                            Id = ti.Id,
                            Title = ti.Title
                        }).ToList()
                        : null
                })
                .ToListAsync();

            return Ok(new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Data = lists
            });
        }

        // GET: api/todolists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoList>> GetTodoList(long id)
        {
            // Include the related TodoItems
            var todoList = await _context.TodoList
                .Include(tl => tl.TodoItems)
                .FirstOrDefaultAsync(tl => tl.Id == id);

            if (todoList == null)
            {
                return NotFound();
            }

            return Ok(todoList);
        }

        // PUT: api/todolists/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoList(long id, UpdateTodoListDto payload)
        {

            var todoList = await _context.TodoList.FindAsync(id);

            if (todoList == null)
            {
                return NotFound();
            }

            todoList.Name = payload.Name;
            await _context.SaveChangesAsync();

            return Ok(todoList);
        }

        // POST: api/todolists
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoList>> PostTodoList(CreateTodoListDto payload)
        {
            var todoList = new TodoList { Name = payload.Name };

            _context.TodoList.Add(todoList);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodoList", new { id = todoList.Id }, todoList);
        }

        // DELETE: api/todolists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoList(long id)
        {
            var todoList = await _context.TodoList.FindAsync(id);
            if (todoList == null)
            {
                return NotFound();
            }

            _context.TodoList.Remove(todoList);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoListExists(long id)
        {
            return (_context.TodoList?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
