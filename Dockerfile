# Multi-stage build: compile with the SDK image, run on the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SkyBook.Api/*.csproj SkyBook.Api/
RUN dotnet restore SkyBook.Api/SkyBook.Api.csproj

COPY SkyBook.Api/ SkyBook.Api/
RUN dotnet publish SkyBook.Api/SkyBook.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Render injects $PORT at container start (not build time), so it must be
# expanded by a shell here rather than baked into a Dockerfile ENV.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet SkyBook.Api.dll"]
