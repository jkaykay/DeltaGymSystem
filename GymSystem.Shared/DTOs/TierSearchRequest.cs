using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for searching and filtering subscription tiers with pagination.
public record TierSearchRequest
{
    // Optional keyword to search by tier name (max 256 characters).
    [MaxLength(256)]
    public string? Search { get; init; }

    // Column name to sort results by (max 50 characters).
    [MaxLength(50)]
    public string? SortBy { get; init; }

    // Sort direction: 'asc' or 'desc' (max 4 characters).
    [MaxLength(4)]
    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
    public string? SortDir { get; init; }

    // Page number for pagination (default 1).
    public int Page { get; init; } = 1;

    // Number of items per page (default 10).
    public int PageSize { get; init; } = 10;
}
