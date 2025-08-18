using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Interfaces;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlServer(
        "Server=(localdb)\\mssqllocaldb;Database=Todos;Trusted_Connection=True;MultipleActiveResultSets=true"
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
//{
//    client.BaseAddress = new Uri("https://external-api-url/");
//});

builder.Services.AddSingleton<IExternalApiClient, MockExternalApiClient>();

builder.Services.AddSingleton<ITodoSyncService, TodoSyncService>();
builder.Services.AddHostedService<TodoSyncService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.Migrate();
}

app.Run();
