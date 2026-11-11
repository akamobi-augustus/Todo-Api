using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo_Api.Data;
using Todo_Api.Dtos;
using Todo_Api.Models;

namespace Todo_Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TodoResponse>>> GetAll(
        [FromQuery] TodoQuery q, CancellationToken ct)
    {
        var page = Math.Max(q.Page, 1);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var now = DateTime.UtcNow;

        var query = db.Todos.AsNoTracking().Include(t => t.Category).AsQueryable();

        if (q.Status is not null)
            query = query.Where(t => t.Status == q.Status);

        if (q.CategoryId is not null)
            query = query.Where(t => t.CategoryId == q.CategoryId);

        if (q.Overdue == true)
            query = query.Where(t => t.DueDate != null && t.DueDate < now && t.Status != TodoStatus.Done);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.DueDate == null)   // tasks with a due date first
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<TodoResponse>(items.Select(ToResponse).ToList(), page, pageSize, total);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoResponse>> GetById(int id, CancellationToken ct)
    {
        var todo = await db.Todos.AsNoTracking().Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return todo is null ? NotFound() : ToResponse(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create(TodoRequest request, CancellationToken ct)
    {
        if (!await CategoryOk(request.CategoryId, ct))
            return BadRequest(new { error = $"Category {request.CategoryId} does not exist." });

        var todo = new TodoItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DueDate = request.DueDate?.ToUniversalTime(),
            Status = request.Status,
            CategoryId = request.CategoryId
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync(ct);
        await db.Entry(todo).Reference(t => t.Category).LoadAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, ToResponse(todo));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TodoResponse>> Update(int id, TodoRequest request, CancellationToken ct)
    {
        var todo = await db.Todos.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (todo is null) return NotFound();

        if (!await CategoryOk(request.CategoryId, ct))
            return BadRequest(new { error = $"Category {request.CategoryId} does not exist." });

        todo.Title = request.Title.Trim();
        todo.Description = request.Description?.Trim();
        todo.DueDate = request.DueDate?.ToUniversalTime();
        todo.Status = request.Status;
        todo.CategoryId = request.CategoryId;
        todo.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await db.Entry(todo).Reference(t => t.Category).LoadAsync(ct);

        return ToResponse(todo);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TodoResponse>> UpdateStatus(int id, StatusRequest request, CancellationToken ct)
    {
        var todo = await db.Todos.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (todo is null) return NotFound();

        todo.Status = request.Status;
        todo.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return ToResponse(todo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var todo = await db.Todos.FindAsync([id], ct);
        if (todo is null) return NotFound();

        db.Todos.Remove(todo);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<bool> CategoryOk(int? categoryId, CancellationToken ct) =>
        categoryId is null || await db.Categories.AnyAsync(c => c.Id == categoryId, ct);

    private static TodoResponse ToResponse(TodoItem t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.DueDate,
        t.Status,
        t.DueDate is not null && t.DueDate < DateTime.UtcNow && t.Status != TodoStatus.Done,
        t.CategoryId,
        t.Category?.Name,
        t.CreatedAt,
        t.UpdatedAt);
}