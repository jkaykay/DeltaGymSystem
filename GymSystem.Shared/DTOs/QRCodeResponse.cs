namespace GymSystem.Shared.DTOs;

public record QRCodeResponse(
    string QrCodeBase64,
    DateTime ExpiresAt
    // Display as: <img src="data:image/png;base64,{QrCodeBase64}" />
);