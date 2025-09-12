namespace QRAlbums.API.Options;

public class BunnyOptions
{
    public string PullZoneBaseUrl { get; set; } = string.Empty;
    public string SecurityKey { get; set; } = string.Empty;
    public bool OptimizerEnabled { get; set; }
    public int DefaultQuality { get; set; } = 80;
    public WatermarkOptions Watermark { get; set; } = new();
    public int URLExpirySeconds { get; set; } = 600;
    public string? StreamBaseUrl { get; set; }

    public class WatermarkOptions
    {
        public bool Enabled { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Opacity { get; set; }
        public int Size { get; set; }
    }
}
