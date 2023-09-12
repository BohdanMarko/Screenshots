using Microsoft.AspNetCore.Mvc;
using Screenshots.Models;
using Screenshots.Services;
using Screenshots.Settings;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection(nameof(ApplicationSettings)));

builder.Services.AddScoped<IScreenshotsService, ScreenshotsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
	try
	{
		await next(context);
	}
	catch (Exception ex)
	{
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(new ErrorDetails(
            StatusCode: context.Response.StatusCode,
            Message: "Something went wrong! " + ex.Message
            ).ToString());
    }
});

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Content(File.ReadAllText(@".\assets\index.html"), "text/html"));

app.MapPost("/screenshot", async (IScreenshotsService screenshotsService, [FromBody]TakeScreenshotRequest request) =>
{
    var response = await screenshotsService.TakeScreenshot(request.TargetUrl);
    return Results.File(response.Content, "image/png", response.FileName);
});

app.Run();