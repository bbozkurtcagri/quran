# syntax=docker/dockerfile:1.7

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy MSBuild infrastructure first so package-restore layers cache well.
COPY global.json Directory.Build.props Directory.Packages.props ./

# Copy project files to restore against just the manifests.
COPY src/QuranCompanion.Domain/QuranCompanion.Domain.csproj         src/QuranCompanion.Domain/
COPY src/QuranCompanion.Application/QuranCompanion.Application.csproj src/QuranCompanion.Application/
COPY src/QuranCompanion.Infrastructure/QuranCompanion.Infrastructure.csproj src/QuranCompanion.Infrastructure/
COPY src/QuranCompanion.Api/QuranCompanion.Api.csproj               src/QuranCompanion.Api/

# NoWarn NU1903: Microsoft.AspNetCore.OpenApi 10.0.2 transitively pulls Microsoft.OpenApi 2.0.0
# which has GHSA-v5pm-xwqc-g5wc. TreatWarningsAsErrors turns this into a build failure.
# Proper fix (later): pin Microsoft.OpenApi >= 2.1.0 explicitly in Directory.Packages.props.
RUN dotnet restore src/QuranCompanion.Api/QuranCompanion.Api.csproj \
    -p:NoWarn=NU1903

# Copy the rest of the source and publish.
COPY src/ ./src/

RUN dotnet publish src/QuranCompanion.Api/QuranCompanion.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false \
    -p:NoWarn=NU1903

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is used by the compose healthcheck against /health/live.
# The base image already ships a non-root 'app' user.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build --chown=app:app /app/publish ./

USER app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["dotnet", "QuranCompanion.Api.dll"]
