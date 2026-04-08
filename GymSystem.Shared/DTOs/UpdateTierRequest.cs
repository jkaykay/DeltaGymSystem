using System.ComponentModel.DataAnnotations;

namespace GymSystem.Shared.DTOs;

// Request DTO for updating a subscription tier's price.
// Price: Updated price (0.01–99999.99, null to keep current).
public record UpdateTierRequest(
    [Range(0.01, 99999.99)] decimal? Price = null
);
