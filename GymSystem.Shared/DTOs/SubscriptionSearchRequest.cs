using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for searching and filtering subscriptions with pagination.
public record SubscriptionSearchRequest
{
    // Optional keyword to search by member name or tier (max 256 characters).
    [MaxLength(256)]
    public string? Search { get; init; }

    // Optional filter by subscription state (integer maps to SubscriptionState enum).
    public int? State { get; init; }

    // Optional filter by tier name (max 25 characters).
    [MaxLength(25)]
    public string? TierName { get; init; }

    // Optional lower bound for subscription start date.
    public DateTime? StartFrom { get; init; }

    // Optional upper bound for subscription start date.
    public DateTime? StartTo { get; init; }

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
