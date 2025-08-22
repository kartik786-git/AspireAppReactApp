using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using AspireAppReactApp.ApiService;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/Todo").WithTags(nameof(Todo));

        group.MapGet("/", async (TodoDbContext db) =>
        {
            return await db.Todo.OrderBy(t => t.Position).ToListAsync();
        })
        .WithName("GetAllTodos")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Todo>, NotFound>> (int id, TodoDbContext db) =>
        {
            return await db.Todo.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Todo model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetTodoById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Todo todo, TodoDbContext db) =>
        {
            var affected = await db.Todo
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Title, todo.Title)
                .SetProperty(m => m.IsComplete, todo.IsComplete)
                .SetProperty(m => m.Position, todo.Position)
        );

            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateTodo")
        .WithOpenApi();

        group.MapPost("/", async (Todo todo, TodoDbContext db) =>
        {
            if (todo.Position <= 0)
            {
                // If position is not set, assign it to the next available position
                todo.Position = await db.Todo.AnyAsync()
                    ? await db.Todo.MaxAsync(t => t.Position) + 1
                    : 1; // Start at position 1 if no todos exist
            }
            db.Todo.Add(todo);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Todo/{todo.Id}",todo);
        })
        .WithName("CreateTodo")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, TodoDbContext db) =>
        {
            var affected = await db.Todo
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();

            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteTodo")
        .WithOpenApi();

        // Endpoint to move a task up in the list
        group.MapPost("/move-up/{id:int}", async Task<Results<Ok, NotFound>> (int id, TodoDbContext db) =>
        {
            var todo = await db.Todo.FirstOrDefaultAsync(t => t.Id == id);
            if (todo is null)
            { return TypedResults.NotFound(); }

            // Find the todo with the largest position less than the current todo
            var prevTodo = await db.Todo
                .Where(t => t.Position < todo.Position)
                .OrderByDescending(t => t.Position)
                .FirstOrDefaultAsync();

            if (prevTodo is null)
            { return TypedResults.Ok(); }

            // Swap positions
            (todo.Position, prevTodo.Position) = (prevTodo.Position, todo.Position);
            await db.SaveChangesAsync();
            return TypedResults.Ok();
        })
        .WithName("MoveTaskUp");

        // Endpoint to move a task down in the list
        group.MapPost("/move-down/{id:int}", async Task<Results<Ok, NotFound>> (int id, TodoDbContext db) =>
        {
            var todo = await db.Todo.FirstOrDefaultAsync(t => t.Id == id);
            if (todo is null)
            { return TypedResults.NotFound(); }

            // Find the todo with the smallest position greater than the current todo
            var nextTodo = await db.Todo
                .Where(t => t.Position > todo.Position)
                .OrderBy(t => t.Position)
                .FirstOrDefaultAsync();

            if (nextTodo is null)
            { return TypedResults.Ok(); } // Already at the bottom or no next todo

            // Swap positions values
            (todo.Position, nextTodo.Position) = (nextTodo.Position, todo.Position);
            await db.SaveChangesAsync();
            return TypedResults.Ok();
        })
        .WithName("MoveTaskDown");
    }

}
