namespace GymSystem.Shared.DTOs;

public record UpdatePaymentRequest(
    decimal? Amount,
    DateTime? PaymentDate,
    string? UserId,
    int? SubId 
    );
