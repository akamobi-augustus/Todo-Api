using System.ComponentModel.DataAnnotations;

namespace Todo_Api.Dtos;

public class CategoryRequest
{
    [Required, StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}

public record CategoryResponse(int Id, string Name, int TodoCount);