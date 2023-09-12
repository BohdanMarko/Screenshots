using Microsoft.Extensions.Options;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using Screenshots.Settings;
using Screenshots.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Globalization;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Screenshots.Services;

public sealed class ScreenshotsService : IScreenshotsService
{
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<ScreenshotsService> _logger;   
    private readonly DateTime dateTimeNow = DateTime.Now;

    private const string FILE_NAME_DATE_TIME_FORMAT = "yyyyMMddHHmmss";
    private const int FONT_SIZE_OFFSET = 5;

    public ScreenshotsService(
        IOptions<ApplicationSettings> options, 
        ILogger<ScreenshotsService> logger)
    {
        _appSettings = options.Value;
        _logger = logger;
    }

    public async Task<TakeScreenshotResponse> TakeScreenshot(string targetUrl)
    {
        try
        {
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            string fileName = string.Format(
                _appSettings.BaseFileName,
                dateTimeNow.ToString(FILE_NAME_DATE_TIME_FORMAT));

            var launchOptions = new LaunchOptions() { Headless = false };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();
            await page.GoToAsync(targetUrl);
            await page.SetViewportAsync(new ViewPortOptions 
            { 
                Height = _appSettings.ScreenshotHeight, 
                Width = _appSettings.ScreenshotWidth
            });

            using var screenshotStream = await page.ScreenshotStreamAsync(new ScreenshotOptions
            {
                Clip = new Clip
                {
                    Height = _appSettings.ScreenshotHeight,
                    Width = _appSettings.ScreenshotWidth
                }
            });

            using MemoryStream resultStream = await TimeStampScreenshot(screenshotStream);

            return new TakeScreenshotResponse(fileName, resultStream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error taking screenshot");
            throw;
        }
    }

    private async Task<MemoryStream> TimeStampScreenshot(Stream inputStream)
    {
        try
        {
            Image inputImage = await Image.LoadAsync(inputStream);

            int height = inputImage.Height + _appSettings.FontSize + FONT_SIZE_OFFSET;
            using Image<Rgba32> outputImage = new(inputImage.Width, height);

            string timestamp = dateTimeNow.ToString(
                _appSettings.TimeStampFormat,
                CultureInfo.GetCultureInfo(_appSettings.CultureName));

            Font font = SystemFonts.CreateFont(_appSettings.FontName, _appSettings.FontSize);

            var textOptions = new RichTextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            int yCoordinate = _appSettings.FontSize + FONT_SIZE_OFFSET;
            outputImage.Mutate(img => img
                .DrawImage(inputImage, new SixLabors.ImageSharp.Point(0, yCoordinate), opacity: 1f)
                .DrawText(textOptions, timestamp, Color.Red)
            );

            using var resultStream = new MemoryStream();
            await outputImage.SaveAsPngAsync(resultStream);
            resultStream.Position = 0;
            return resultStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error timestamping screenshot");
            throw;
        }
    }
}
