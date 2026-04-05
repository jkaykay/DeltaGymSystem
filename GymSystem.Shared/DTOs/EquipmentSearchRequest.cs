using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record EquipmentSearchRequest
{
    [MaxLength(256)]
    public string? Search { get; init; }

    public bool? Operational { get; init; }

    public int? RoomId { get; init; }

    public DateTime? DateFrom { get; init; }

    public DateTime? DateTo { get; init; }

    [MaxLength(50)]
    public string? SortBy { get; init; }

    [MaxLength(4)]
    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
    public string? SortDir { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
