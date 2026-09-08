namespace PhoneBrowser.Desktop.Models;

public record DeviceInfoDto(string deviceId, string deviceName, int protocolVersion);

public record PairingRequestDto(string requestId, DeviceInfoDto requester);

public record PairingStatusResponseDto(string requestId, string status, string? pairingToken);