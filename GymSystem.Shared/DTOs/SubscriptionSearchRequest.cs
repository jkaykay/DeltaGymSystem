using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record SubscriptionSearchRequest
{
    [MaxLength(256)]
    public string? Search { get; init; }

    public int? State { get; init; }

    [MaxLength(25)]
    public string? TierName { get; init; }

    public DateTime? StartFrom { get; init; }

    public DateTime? StartTo { get; init; }

    [MaxLength(50)]
    public string? SortBy { get; init; }

    [MaxLength(4)]
    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
    public string? SortDir { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
