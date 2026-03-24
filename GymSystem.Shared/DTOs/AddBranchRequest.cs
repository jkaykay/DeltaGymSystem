using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

public record AddBranchRequest(
    [Required, MaxLength(200)] string Address,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(100)] string Province,
    [Required, MaxLength(20)] string PostCode
);