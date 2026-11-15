using System.Net;
using System.Net.Http.Json;

namespace Todo_Api.Tests;

[Collection("api")]
public class TodoTests(ApiFactory factory)
{
    [Fact]
    public async Task Todos_without_a_token_return_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/todos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Due_date_round_trips_as_utc()
    {
        var client = await factory.NewUserAsync();
        var due = new DateTime(2026, 12, 20, 18, 0, 0, DateTimeKind.Utc);

        var created = await client.CreateTodoAsync("Ship it", due);
        var fetched = await client.GetFromJsonAsync<TodoDto>($"/api/todos/{created.Id}");

        Assert.Equal(due, fetched!.DueDate!.Value.ToUniversalTime());
    }

    [Fact]
    public async Task Empty_title_returns_400()
    {
        var client = await factory.NewUserAsync();

        var response = await client.PostAsJsonAsync("/api/todos", new { title = "", status = "Pending" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_status_returns_400()
    {
        var client = await factory.NewUserAsync();

        var response = await client.PostAsJsonAsync("/api/todos", new { title = "Task", status = "Banana" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Overdue_filter_returns_only_late_unfinished_tasks()
    {
        var client = await factory.NewUserAsync();
        await client.CreateTodoAsync("Late", DateTime.UtcNow.AddDays(-3));
        await client.CreateTodoAsync("Future", DateTime.UtcNow.AddDays(3));
        await client.CreateTodoAsync("Late but done", DateTime.UtcNow.AddDays(-3), status: "Done");

        var page = await client.GetFromJsonAsync<PagedDto>("/api/todos?overdue=true");

        var only = Assert.Single(page!.Items);
        Assert.Equal("Late", only.Title);
        Assert.True(only.IsOverdue);
    }

    [Fact]
    public async Task Search_ignores_case()
    {
        var client = await factory.NewUserAsync();
        await client.CreateTodoAsync("Pay Electricity Bill");
        await client.CreateTodoAsync("Buy milk");

        var page = await client.GetFromJsonAsync<PagedDto>("/api/todos?search=electricity");

        var only = Assert.Single(page!.Items);
        Assert.Equal("Pay Electricity Bill", only.Title);
    }

    [Fact]
    public async Task Paging_returns_the_right_slice()
    {
        var client = await factory.NewUserAsync();
        for (var i = 1; i <= 3; i++)
            await client.CreateTodoAsync($"Task {i}");

        var page2 = await client.GetFromJsonAsync<PagedDto>("/api/todos?page=2&pageSize=2");

        Assert.Equal(3, page2!.TotalCount);
        Assert.Single(page2.Items);
    }

    [Fact]
    public async Task Patch_status_updates_the_task()
    {
        var client = await factory.NewUserAsync();
        var todo = await client.CreateTodoAsync("Task");

        var response = await client.PatchAsync($"/api/todos/{todo.Id}/status",
            JsonContent.Create(new { status = "Done" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.Equal("Done", updated!.Status);
    }

    [Fact]
    public async Task Another_users_task_is_invisible_to_everyone_else()
    {
        var a = await factory.NewUserAsync();
        var b = await factory.NewUserAsync();
        var todo = await a.CreateTodoAsync("Private");

        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/todos/{todo.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/todos/{todo.Id}")).StatusCode);

        var patch = await b.PatchAsync($"/api/todos/{todo.Id}/status",
            JsonContent.Create(new { status = "Done" }));
        Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);

        var theirList = await b.GetFromJsonAsync<PagedDto>("/api/todos");
        Assert.Equal(0, theirList!.TotalCount);

        var mine = await a.GetFromJsonAsync<TodoDto>($"/api/todos/{todo.Id}");
        Assert.Equal("Pending", mine!.Status);
    }

    [Fact]
    public async Task Task_cannot_use_another_users_category()
    {
        var a = await factory.NewUserAsync();
        var b = await factory.NewUserAsync();
        var category = await a.CreateCategoryAsync("Work");

        var response = await b.PostAsJsonAsync("/api/todos",
            new { title = "Sneaky", status = "Pending", categoryId = category.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}