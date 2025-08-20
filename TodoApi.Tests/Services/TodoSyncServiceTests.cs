using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Dtos.External.TodoItems;
using TodoApi.Dtos.External.TodoLists;
using TodoApi.Interfaces;
using TodoApi.Models;
using TodoApi.Services;
using TodoApi.Tests.Helpers;

namespace TodoApi.Tests.Services
{
    public class TodoSyncServiceTests
    {
        private readonly TodoContext _context;
        private readonly Mock<IExternalApiClient> _externalApiMock;

        public TodoSyncServiceTests()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: "TodoTestDb")
                .Options;

            _context = new TodoContext(options);
            _externalApiMock = new Mock<IExternalApiClient>();
        }

        [Fact]
        public async Task SyncFromExternalAsync_ShouldCreateLocalList_WhenNotExists()
        {
            // Arrange
            var externalList = new ExternalTodoListDto
            {
                Id = "ext-1",
                Name = "Test List",
                TodoItems = new List<ExternalTodoItemDto>()
            };

            _externalApiMock.Setup(x => x.ListTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoListDto> { externalList });

            var scopeFactory = new TestScopeFactory(_context);
            var logger = Mock.Of<ILogger<TodoSyncService>>();
            var service = new TodoSyncService(scopeFactory, _externalApiMock.Object, logger);

            // Act
            await service.SyncFromExternalAsync();

            // Assert
            var localList = _context.TodoList.FirstOrDefault();
            Assert.NotNull(localList);
            Assert.Equal("Test List", localList.Name);
            Assert.Equal("ext-1", localList.ExternalId);
        }

        [Fact]
        public async Task SyncFromExternalAsync_ShouldUpdateExistingList_WhenExists()
        {
            // Arrange
            _context.TodoList.Add(new TodoList
            {
                ExternalId = "ext-1",
                Name = "Old Name",
                UpdatedAtUtc = System.DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var externalList = new ExternalTodoListDto
            {
                Id = "ext-1",
                Name = "Updated Name",
                TodoItems = new List<ExternalTodoItemDto>()
            };

            _externalApiMock.Setup(x => x.ListTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoListDto> { externalList });

            var scopeFactory = new TestScopeFactory(_context);
            var logger = Mock.Of<ILogger<TodoSyncService>>();
            var service = new TodoSyncService(scopeFactory, _externalApiMock.Object, logger);

            // Act
            await service.SyncFromExternalAsync();

            // Assert
            var localList = _context.TodoList.FirstOrDefault(l => l.ExternalId == "ext-1");
            Assert.NotNull(localList);
            Assert.Equal("Updated Name", localList.Name);
        }
    }
}
