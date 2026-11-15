using System.Net;
using System.Net.Http.Json;

namespace Todo_Api.Tests;

[Collection("api")]
public class CategoryTests(ApiFactory factory)
{
    [Fact]
    public async Task Duplicate_name_returns_409_but_another_user_can_reuse_it()
    {
        var a = await factory.NewUserAsync();
        var b = await factory.NewUserAsync();
        await a.CreateCategoryAsync("Work");

        var duplicate = await a.PostAsJsonAsync("/api/categories", new { name = "work" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var other = await b.PostAsJsonAsync("/api/categories", new { name = "Work" });
        Assert.Equal(HttpStatusCode.Created, other.StatusCode);
    }

    [Fact]
    public async Task Category_shows_task_count()
    {
        var client = await factory.NewUserAsync();
        var category = await client.CreateCategoryAsync("Home");
        await client.CreateTodoAsync("One", categoryId: category.Id);
        await client.CreateTodoAsync("Two", categoryId: category.Id);

        var list = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");

        var only = Assert.Single(list!);
        Assert.Equal(2, only.TodoCount);
    }

    [Fact]
    public async Task Deleting_a_category_keeps_its_tasks()
    {
        var client = await factory.NewUserAsync();
        var category = await client.CreateCategoryAsync("Work");
        var todo = await client.CreateTodoAsync("Task", categoryId: category.Id);

        var delete = await client.DeleteAsync($"/api/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var fetched = await client.GetFromJsonAsync<TodoDto>($"/api/todos/{todo.Id}");
        Assert.Null(fetched!.CategoryId);
    }
}