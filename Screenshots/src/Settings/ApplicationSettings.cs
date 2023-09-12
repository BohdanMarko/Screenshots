namespace Screenshots.Settings;

public sealed class ApplicationSettings
{
    public required string BaseFileName { get; set; }
    public required string TimeStampFormat { get; set; }
    public required string FontName { get; set; }
    public required int FontSize { get; set; }
    public required int ScreenshotHeight { get; set; }
    public required int ScreenshotWidth { get; set; }
    public required string CultureName { get; set; }
}
