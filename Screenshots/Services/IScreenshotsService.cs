using Screenshots.Models;

namespace Screenshots.Services;

public interface IScreenshotsService
{
    Task<TakeScreenshotResponse> TakeScreenshot(string targetUrl);
}
