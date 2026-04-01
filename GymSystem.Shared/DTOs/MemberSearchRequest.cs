using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record MemberSearchRequest
{
    [MaxLength(256)]
    public string? Search { get; init; }
    public bool? Active { get; init; }
    public DateTime? JoinedFrom { get; init; }
    public DateTime? JoinedTo { get; init; }
    [MaxLength(50)]
    public string? SortBy { get; init; }
    [MaxLength(4)]
    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
    public string? SortDir { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}