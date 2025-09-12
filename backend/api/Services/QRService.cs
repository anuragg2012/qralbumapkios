using QRCoder;

namespace QRAlbums.API.Services;

public class QRService : IQRService
{
    public string GenerateQRCodeBase64(string url, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        var qrCodeImage = qrCode.GetGraphic(pixelsPerModule);
        return Convert.ToBase64String(qrCodeImage);
    }
}