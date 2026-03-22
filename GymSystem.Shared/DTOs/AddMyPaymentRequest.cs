namespace GymSystem.Shared.DTOs;

public record AddMyPaymentRequest(
    decimal Amount,
    int SubId
);
