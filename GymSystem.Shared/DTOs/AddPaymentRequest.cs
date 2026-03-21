namespace GymSystem.Shared.DTOs;

public record AddPaymentRequest(
    decimal Amount,
    string UserId,
    int SubId
    );