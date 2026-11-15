using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Todo_Api.Tests;

public record AuthResult(string Token, DateTime ExpiresAt, string Email);

public record TodoDto(int Id, string Title, string? Description, DateTime? DueDate, string Status,
                      bool IsOverdue, int? CategoryId, string? CategoryName);

public record PagedDto(List<TodoDto> Items, int Page, int PageSize, int TotalCount);

public record CategoryDto(int Id, string Name, int TodoCount);

public static class ApiExtensions
{
    public static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";

    // Registers a brand-new user and returns a client that is already logged in.
    public static async Task<HttpClient> NewUserAsync(this ApiFactory factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = NewEmail(), password = "StrongPass123" });
        response.EnsureSuccessStatusCode();

        var auth = (await response.Content.ReadFromJsonAsync<AuthResult>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    public static async Task<TodoDto> CreateTodoAsync(this HttpClient client, string title,
        DateTime? dueDate = null, string status = "Pending", int? categoryId = null)
    {
        var response = await client.PostAsJsonAsync("/api/todos", new { title, dueDate, status, categoryId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TodoDto>())!;
    }

    public static async Task<CategoryDto> CreateCategoryAsync(this HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }
}