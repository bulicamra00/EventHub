namespace EventHub.Domain.Interfaces;

public interface IQrCodeService
{
    string GenerateQrCode(string data);
}