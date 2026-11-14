# Todo-Api

A REST API and web interface for managing personal tasks, with JWT authentication. Built with ASP.NET Core and Entity Framework Core.

## Tech Stack
C# · ASP.NET Core (.NET 10) · Entity Framework Core · SQLite · JWT Bearer authentication · Swagger (Swashbuckle) · HTML, CSS and JavaScript

## Features
- Register and log in; passwords are hashed, never stored in plain text
- JWT tokens with expiry; all task and category endpoints require a login
- Every user sees only their own tasks and categories (other users' data returns 404)
- Full CRUD for tasks and categories
- Status tracking: `Pending`, `InProgress`, `Done`
- Due dates with automatic overdue detection
- Filtering by status, category and overdue; title search; paging
- Input validation with proper HTTP status codes (400, 401, 404, 409)
- Web interface served by the API itself, with a login screen
- EF Core migrations; the database is created automatically on first run

## Getting Started

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```
git clone git@github.com:akamobi-augustus/Todo-Api.git
cd Todo-Api
```

The token signing key is a secret and is not in the repository. Create your own once:

```
$b = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($b))
```

Then run:

```
dotnet run
```

Open the URL printed in the terminal (for example `http://localhost:5093`):

- `/` opens the web interface (create an account, then log in)
- `/swagger` opens the API docs. Use **Authorize** and paste the token from `/api/auth/login`

## Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | /api/auth/register | No | Create an account, returns a token |
| POST | /api/auth/login | No | Log in, returns a token |
| GET | /api/auth/me | Yes | Current user |
| GET | /api/todos | Yes | List tasks (filters: `status`, `categoryId`, `overdue`, `search`, `page`, `pageSize`) |
| GET | /api/todos/{id} | Yes | Get one task |
| POST | /api/todos | Yes | Create a task |
| PUT | /api/todos/{id} | Yes | Update a task |
| PATCH | /api/todos/{id}/status | Yes | Change status only |
| DELETE | /api/todos/{id} | Yes | Delete a task |
| GET | /api/categories | Yes | List categories with task counts |
| GET | /api/categories/{id} | Yes | Get one category |
| POST | /api/categories | Yes | Create a category |
| PUT | /api/categories/{id} | Yes | Rename a category |
| DELETE | /api/categories/{id} | Yes | Delete a category |

## Example Requests

`POST /api/auth/register`
```json
{ "email": "demo@example.com", "password": "StrongPass123" }
```

`POST /api/todos` (send the token as `Authorization: Bearer <token>`)
```json
{
  "title": "Finish URL shortener",
  "description": "Add stats endpoint",
  "dueDate": "2026-12-20T18:00:00Z",
  "status": "Pending",
  "categoryId": 1
}
```

## Project Structure
```
Controllers/   HTTP endpoints (auth, todos, categories)
Services/      Token creation and helpers
Data/          EF Core DbContext
Dtos/          Request and response models
Models/        Entities and enums
Migrations/    Database migrations
wwwroot/       Web interface (index.html)
```

## Security Notes
- Passwords are hashed with ASP.NET Core's `PasswordHasher`
- The signing key is read from user-secrets or environment variables, never from the repo
- Login errors do not reveal whether an email exists
- The web interface stores the token in `localStorage`; a hardening step would move it to an HttpOnly cookie

## Roadmap
- [x] Web interface
- [x] JWT authentication with per-user data
- [ ] Move to PostgreSQL
- [ ] Integration tests with Testcontainers (xUnit)
- [ ] Dockerfile and GitHub Actions CI
- [ ] HttpOnly cookie sessions and refresh tokens

## Author
Augustus Akamobi · [GitHub](https://github.com/akamobi-augustus)