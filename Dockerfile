# Stage 1: Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

# Copy NuGet config and solution
COPY NuGet.Config ./
COPY ComplyEA.sln ./

# Copy project files for restore
COPY ComplyEA.Module/ComplyEA.Module.csproj ComplyEA.Module/
COPY ComplyEA.Module.Blazor/ComplyEA.Module.Blazor.csproj ComplyEA.Module.Blazor/
COPY ComplyEA.Blazor.Server/ComplyEA.Blazor.Server.csproj ComplyEA.Blazor.Server/

# Restore packages (requires DevExpress NuGet credentials via build args)
ARG DEVEXPRESS_NUGET_KEY
ARG DEVEXPRESS_NUGET_USER=DevExpress
ENV DEVEXPRESS_NUGET_KEY=${DEVEXPRESS_NUGET_KEY}
ENV DEVEXPRESS_NUGET_USER=${DEVEXPRESS_NUGET_USER}
RUN dotnet restore ComplyEA.sln

# Copy everything else and build
COPY . .
RUN dotnet publish ComplyEA.Blazor.Server/ComplyEA.Blazor.Server.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app

# Install PostgreSQL client library for Npgsql
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "ComplyEA.Blazor.Server.dll"]
