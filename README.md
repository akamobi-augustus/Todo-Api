# Todo-Api

A REST API for managing tasks with categories, due dates and statuses, built with ASP.NET Core and Entity Framework Core.

## Tech Stack
C# · ASP.NET Core (.NET 9) · Entity Framework Core · SQLite · Swagger (Swashbuckle)

## Features
- Full CRUD for tasks and categories
- Status tracking: `Pending`, `InProgress`, `Done`
- Due dates with automatic overdue detection
- Filtering by status, category and overdue; title search; paging
- Input validation with proper HTTP status codes (400, 404, 409)
- Deleting a category keeps its tasks (they become uncategorised)
- EF Core migrations; the database is created automatically on first run

## Getting Started

Requirements: [.NET 9 SDK](https://dotnet.microsoft.com/download)

```
git clone git@github.com:akamobi-augustus/REPO-NAME.git
cd REPO-NAME
dotnet run
```

Open the URL printed in the terminal and add `/swagger` (for example `http://localhost:5093/swagger`).

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/todos | List tasks (filters: `status`, `categoryId`, `overdue`, `search`, `page`, `pageSize`) |
| GET | /api/todos/{id} | Get one task |
| POST | /api/todos | Create a task |
| PUT | /api/todos/{id} | Update a task |
| PATCH | /api/todos/{id}/status | Change status only |
| DELETE | /api/todos/{id} | Delete a task |
| GET | /api/categories | List categories with task counts |
| GET | /api/categories/{id} | Get one category |
| POST | /api/categories | Create a category |
| PUT | /api/categories/{id} | Rename a category |
| DELETE | /api/categories/{id} | Delete a category |

## Example Request

`POST /api/todos`
```json
{
  "title": "Finish URL shortener",
  "description": "Add stats endpoint",
  "dueDate": "2026-11-20T18:00:00Z",
  "status": "Pending",
  "categoryId": 1
}
```

## Project Structure
```
Controllers/   HTTP endpoints
Data/          EF Core DbContext
Dtos/          Request and response models
Models/        Entities and enums
Migrations/    Database migrations
```

## Roadmap
- [ ] Service layer and unit tests (xUnit)
- [ ] JWT authentication (each user sees only their own tasks)
- [ ] Dockerfile and GitHub Actions CI

## Author
Augustus Akamobi · [GitHub](https://github.com/akamobi-augustus)