namespace GymSystem.Shared.DTOs;

// Response DTO containing a QR code image as a base64-encoded PNG.
// QrCodeBase64: Base64-encoded PNG image of the QR code.
// ExpiresAt: Date and time when the QR code expires.
public record QRCodeResponse(
    string QrCodeBase64,
    DateTime ExpiresAt
    // Display as: <img src="data:image/png;base64,{QrCodeBase64}" />
);
