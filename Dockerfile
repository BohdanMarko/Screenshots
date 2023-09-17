# Use the official .NET SDK image as a build stage.
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory in the container.
WORKDIR /app

# Copy the project files into the container.
COPY ./src/Screenshots/ .

# Build the application and publish it to the "out" folder.
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image for the final stage.
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory in the container.
WORKDIR /app

# Copy the published application from the build stage to the runtime stage.
COPY --from=build /app/out ./

# Expose the port that the application will listen on.
EXPOSE 80

# Define the entry point for the application.
ENTRYPOINT ["dotnet", "Screenshots.dll"]
