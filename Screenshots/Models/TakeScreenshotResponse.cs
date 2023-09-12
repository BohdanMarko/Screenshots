namespace Screenshots.Models;

public sealed record TakeScreenshotResponse(string FileName, byte[] Content);
