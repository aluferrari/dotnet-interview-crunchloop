using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Dtos.External.TodoLists;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services;

namespace TodoApi.Tests.Services
{
    public class ExternalApiClientTests
    {
        [Fact]
        public async Task CreateTodoListAsync_FailedHttpRequest_LogsFailedJob()
        {
            // Arrange
            var dto = new ExternalCreateTodoListDto { Name = "Test List" };

            // Mock HttpMessageHandler to simulate HTTP failure
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new System.Uri("https://example.com/")
            };

            // Mock repository
            var repoMock = new Mock<FailedSyncJobRepository>();
            repoMock.Setup(r => r.SaveAsync(It.IsAny<FailedSyncJob>())).Returns(Task.CompletedTask);

            var client = new ExternalApiClient(httpClient, repoMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.CreateTodoListAsync(dto));

            // Verify DLQ was called
            repoMock.Verify(r => r.SaveAsync(It.Is<FailedSyncJob>(
                f => f.Operation == "CreateTodoList" &&
                     f.Payload.Contains("Test List")
            )), Times.Once);
        }

        [Fact]
        public async Task ListTodoListsAsync_ReturnsData()
        {
            var responseContent = "[{\"Id\":\"1\",\"Name\":\"Test\"}]";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new System.Uri("https://example.com/")
            };

            var repoMock = new Mock<FailedSyncJobRepository>();
            var client = new ExternalApiClient(httpClient, repoMock.Object);

            var result = await client.ListTodoListsAsync();

            Assert.Single(result);
            Assert.Equal("1", result[0].Id);
            repoMock.Verify(r => r.SaveAsync(It.IsAny<FailedSyncJob>()), Times.Never);
        }
    }
}
