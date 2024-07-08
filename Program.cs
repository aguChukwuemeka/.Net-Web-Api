using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskServices>(new InMemoryTaskService());

var app = builder.Build();

var todoList = new List<Todo>();

app.UseRewriter(new RewriteOptions().AddRedirect("task/.*", "/todos/1"));

app.Use(async (context, next) =>
{
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.Now.ToString()} {DateTime.UtcNow.ToString()}] Started");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.Now.ToString()} {DateTime.UtcNow.ToString()}] Finished");
});

// app.MapGet("/todos", () => todoList);
app.MapGet("/todos", (ITaskServices services) => services.GetTodos());

app.MapGet("/todos/{id}", Results < Ok<Todo>, NotFound> (int id) =>
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
// .AddEndpointFilter(async (context, next) => {
//     var taskArguments = context.GetArgument<Todo>(0);
//     var error = new Dictionary<string, string[]>();
//     if (taskArguments.DueTime < DateTime.UtcNow)
//     {
//         error.Add(nameof(Todo.DueTime), [ "Cannot have due date in the past" ]);
//     }
//     if(taskArguments.IsCompleted){
//         error.Add(nameof(Todo.IsCompleted), [ "Cannot set completed status to true for a task with a due date in the past" ]);
//     }
//     if(error.Count > 0){
//         return  Results.ValidationProblem(error);
//     }
// });

app.MapDelete("/todos/{id}", (int id) =>
{
    todoList.RemoveAll(item => item.id == id);
    return TypedResults.NoContent();
});

app.Run();

public record Todo(int id, string Name, DateTime DueTime, bool IsCompleted) { }

interface ITaskServices{
    Todo? GetTodoById(int id);
    List<Todo> GetTodos();
    Todo AddTodo(Todo task);
    void UpdateTodo(Todo updatedTodo);
    void DeleteTodo(int id);
}


class InMemoryTaskService : ITaskServices{

    private readonly List<Todo> _todos = [];
    public Todo AddTodo(Todo task){
        _todos.Add(task);
        return task;
    }

    public void DeleteTodo(int id){
        _todos.RemoveAll(item => item.id == id);
    }

    public Todo? GetTodoById(int id){
        return _todos.SingleOrDefault(item => item.id == id);
    }
    
    public List<Todo> GetTodos(){
        return _todos;
    }
    public void UpdateTodo(Todo task){
        var todoToUpdate = _todos.SingleOrDefault(item => item.id == task.id);
        if(todoToUpdate is not null){
            todoToUpdate = task;
        }
    }
}
