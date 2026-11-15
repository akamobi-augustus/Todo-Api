using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Todo_Api.Tests;

[Collection("api")]
public class AuthTests(ApiFactory factory)
{
    [Fact]
    public async Task Register_returns_201_and_a_token()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = ApiExtensions.NewEmail(), password = "StrongPass123" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
    }

    [Fact]
    public async Task Register_same_email_returns_409_even_with_different_case()
    {
        var client = factory.CreateClient();
        var email = ApiExtensions.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = "StrongPass123" });

        var again = await client.PostAsJsonAsync("/api/auth/register",
            new { email = email.ToUpperInvariant(), password = "StrongPass123" });

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task Register_with_short_password_returns_400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = ApiExtensions.NewEmail(), password = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_with_invalid_email_returns_400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "not-an-email", password = "StrongPass123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_correct_password_returns_a_token()
    {
        var client = factory.CreateClient();
        var email = ApiExtensions.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = "StrongPass123" });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "StrongPass123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var client = factory.CreateClient();
        var email = ApiExtensions.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = "StrongPass123" });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword999" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_unknown_email_returns_401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = ApiExtensions.NewEmail(), password = "StrongPass123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_without_a_token_returns_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_a_token_returns_the_user()
    {
        var client = factory.CreateClient();
        var email = ApiExtensions.NewEmail();
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "StrongPass123" });
        var auth = (await register.Content.ReadFromJsonAsync<AuthResult>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(email, await response.Content.ReadAsStringAsync());
    }
}