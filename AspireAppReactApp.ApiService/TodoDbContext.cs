using Microsoft.EntityFrameworkCore;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<AspireAppReactApp.ApiService.Todo> Todo { get; set; } = default!;
}
