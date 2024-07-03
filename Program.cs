using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var todoList = new List<Todo>();

app.MapGet("/todos", () => todoList);

app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) =>
{
    var targetTodo = todoList.SingleOrDefault(t => t.id == id);
    return targetTodo is null ? TypedResults.NotFound() : TypedResults.Ok(targetTodo);
});

app.MapPut("/todos/{id}", (Todo updatedTask, int id) =>
{
    var targetTodo = todoList.SingleOrDefault(t => t.id == id);
    if (targetTodo == null) 
    {
        return Results.NotFound();
    }

    targetTodo = targetTodo with { Name = updatedTask.Name, DueTime = updatedTask.DueTime, IsCompleted = updatedTask.IsCompleted };

    return Results.Ok(targetTodo);
});

app.MapPost("/todos", (Todo task) =>
{
    todoList.Add(task);
    return TypedResults.Created("/todos/{id}", task);
});

app.MapDelete("/todos/{id}", (int id) =>
{
    todoList.RemoveAll(item => item.id == id);
    return TypedResults.NoContent();
});

app.Run();

public record Todo(int id, string Name, DateTime DueTime, bool IsCompleted) { }