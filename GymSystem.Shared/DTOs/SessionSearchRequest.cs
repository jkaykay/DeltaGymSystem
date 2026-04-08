using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for searching and filtering sessions with pagination.
public record SessionSearchRequest
{
    // Optional keyword to search by class subject or instructor name (max 256 characters).
    [MaxLength(256)]
    public string? Search { get; init; }

    // Optional filter by instructor identity identifier (max 450 characters).
    [MaxLength(450)]
    public string? InstructorId { get; init; }

    // Optional lower bound for session date filter.
    public DateTime? DateFrom { get; init; }

    // Optional upper bound for session date filter.
    public DateTime? DateTo { get; init; }

    // Optional filter by room identifier.
    public int? RoomId { get; init; }

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
