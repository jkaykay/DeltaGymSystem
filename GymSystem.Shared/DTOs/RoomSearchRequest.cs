using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record RoomSearchRequest
{
    public int? BranchId { get; init; }

    public int? RoomNumber { get; init; }

    [MaxLength(50)]
    public string? SortBy { get; init; }

    [MaxLength(4)]
    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
    public string? SortDir { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
