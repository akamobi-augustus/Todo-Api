using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo_Api.Data;
using Todo_Api.Dtos;
using Todo_Api.Models;
using Todo_Api.Services;

namespace Todo_Api.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        return await db.Categories.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Todos.Count))
            .ToListAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await db.Categories.AsNoTracking()
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Todos.Count))
            .FirstOrDefaultAsync(ct);
        return result is null ? NotFound() : result;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var name = request.Name.Trim();

        if (await db.Categories.AnyAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower(), ct))
            return Conflict(new { error = $"Category '{name}' already exists." });

        var category = new Category { Name = name, UserId = userId };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            new CategoryResponse(category.Id, category.Name, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
        if (category is null) return NotFound();

        var name = request.Name.Trim();
        if (await db.Categories.AnyAsync(
                c => c.UserId == userId && c.Id != id && c.Name.ToLower() == name.ToLower(), ct))
            return Conflict(new { error = $"Category '{name}' already exists." });

        category.Name = name;
        await db.SaveChangesAsync(ct);

        var count = await db.Todos.CountAsync(t => t.CategoryId == id && t.UserId == userId, ct);
        return new CategoryResponse(category.Id, category.Name, count);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
        if (category is null) return NotFound();

        db.Categories.Remove(category);   // its tasks keep existing, with no category
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}