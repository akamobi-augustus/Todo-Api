using System.ComponentModel.DataAnnotations;
using Todo_Api.Models;

namespace Todo_Api.Dtos;

public class TodoRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    public int? CategoryId { get; set; }
}

public class StatusRequest
{
    public TodoStatus Status { get; set; }
}

public class TodoQuery
{
    public TodoStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public bool? Overdue { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record TodoResponse(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    TodoStatus Status,
    bool IsOverdue,
    int? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);