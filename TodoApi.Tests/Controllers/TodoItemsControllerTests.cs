using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Controllers;
using TodoApi.Dtos.TodoItems;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests.Controllers
{
    public class TodoItemsControllerTests
    {
        private TodoContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: "TodoItemsTestDb")
                .Options;

            var context = new TodoContext(options);

            // Seed a TodoList
            if (!context.TodoList.Any())
            {
                context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });
                context.SaveChanges();
            }

            return context;
        }

        [Fact]
        public async Task GetTodoItems_ReturnsAllItemsForList()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Ensure the TodoList exists
            if (!context.TodoList.Any(l => l.Id == 1))
                context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });

            context.TodoItems.Add(new TodoItem { Id = 1, Title = "Item 1", IsCompleted = false, TodoListId = 1 });
            context.TodoItems.Add(new TodoItem { Id = 2, Title = "Item 2", IsCompleted = true, TodoListId = 1 });
            await context.SaveChangesAsync();

            var controller = new TodoItemsController(context);

            // Act
            var actionResult = await controller.GetTodoItems(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<TodoItemDto>>(okResult.Value);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new TodoItemsController(context);

            // Act
            var actionResult = await controller.GetTodoItem(1, 999);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateTodoItem_ReturnsCreatedItem()
        {
            // Arrange
            var context = GetInMemoryContext();
            var controller = new TodoItemsController(context);
            var payload = new CreateTodoItemDto { Title = "New Item", IsCompleted = false };

            // Act
            var actionResult = await controller.CreateTodoItem(1, payload);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var item = Assert.IsType<TodoItemDto>(createdResult.Value);
            Assert.Equal("New Item", item.Title);
            Assert.False(item.IsCompleted);
            Assert.Equal(1, item.TodoListId);
        }
        [Fact]
        public async Task UpdateTodoItem_ReturnsUpdatedItem()
        {
            // Arrange
            var context = GetInMemoryContext();

            // Ensure the TodoList exists
            if (!context.TodoList.Any(l => l.Id == 1))
                context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });

            var item = new TodoItem { Id = 1, Title = "Old Title", IsCompleted = false, TodoListId = 1 };
            context.TodoItems.Add(item);
            await context.SaveChangesAsync();

            var controller = new TodoItemsController(context);
            var payload = new UpdateTodoItemDto { Title = "Updated Title", IsCompleted = true };

            // Act
            var actionResult = await controller.UpdateTodoItem(1, 1, payload);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var updatedItem = Assert.IsType<TodoItemDto>(okResult.Value);
            Assert.Equal("Updated Title", updatedItem.Title);
            Assert.True(updatedItem.IsCompleted);
        }

        [Fact]
        public async Task DeleteTodoItem_RemovesItem()
        {
            // Arrange
            var context = GetInMemoryContext();
            var item = new TodoItem { Id = 1, Title = "Item to Delete", IsCompleted = false, TodoListId = 1 };
            context.TodoItems.Add(item);
            await context.SaveChangesAsync();

            var controller = new TodoItemsController(context);

            // Act
            var actionResult = await controller.DeleteTodoItem(1, 1);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            Assert.Empty(context.TodoItems.Where(i => i.Id == 1));
        }
    }
}
