// Program.cs
using Microsoft.EntityFrameworkCore;
using MinimalApi;

// Create a builder.
var builder = WebApplication.CreateBuilder(args);

// Configure EF to use in-memory database, for testing purpose only.
builder.Services.AddDbContext<TodoDbContext>(
    options => options.UseInMemoryDatabase("TodoItems")
);

// Register a background service with a concrete class
builder.Services.AddHostedService<CheckingProductService>();

// Create a new web app.
using var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0#responses
app.MapGet("/", (Func<string>)(() => "Hello World!"));

app.MapGet("/todos", async (HttpContext http, TodoDbContext todoContext) =>
{
    var todos = await todoContext.TodoItems.ToListAsync();
    return todos;
});

app.MapGet("/todos/{id}", async (HttpContext http, TodoDbContext todoContext, int? id) =>
{
    if (!id.HasValue)
    {
        return Results.BadRequest();
    }

    var todo = await todoContext.TodoItems.FindAsync(id);
    if (todo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(todo);
});

app.MapPost("/todos", async (HttpContext http, TodoDbContext dbContext, TodoItem todo) =>
{
    dbContext.TodoItems.Add(todo);
    await dbContext.SaveChangesAsync();
    http.Response.StatusCode = 201; // Created
});

app.MapPut("/todos/{id}", async (HttpContext http, TodoDbContext dbContext, TodoItem changedTodo, int? id) =>
{
    if (!id.HasValue)
    {
        return Results.BadRequest();
    }

    var exisingTodo = await dbContext.TodoItems.FindAsync(id);
    if (exisingTodo == null)
    {
        return Results.NotFound();
    }

    exisingTodo.Title = changedTodo.Title;
    exisingTodo.IsCompleted = changedTodo.IsCompleted;
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todos/{id}", async (HttpContext http, TodoDbContext dbContext, int? id) =>
{
    if (!id.HasValue)
    {
        return Results.BadRequest();
    }

    var todo = await dbContext.TodoItems.FindAsync(id);
    if (todo == null)
    {
        return Results.NotFound();
    }

    dbContext.TodoItems.Remove(todo);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.Run("http://localhost:3000");

// Define TodoItem model
class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public TodoItem(string title) => Title = title;
}

// Define TodoDbContext
class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions options) : base(options) { }
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}