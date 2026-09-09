# syntax=docker/dockerfile:1
# ==========================================================
# apps/api Dockerfile — ASP.NET Core (Modular Monolith)
# Multi-stage, non-root, minimal runtime image
# Build context: apps/api  (see docker-compose.yml build.context)
# ==========================================================

# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj files first for layer caching, preserving their exact nested
# paths — `COPY src/**/*.csproj ./` looked convenient but silently flattens
# every matched file into /src (no BuildKit version I tested preserves the
# nesting), so dotnet restore couldn't find any of them at the paths
# Marketplace.sln expects. Confirmed by a real build failure during Task 1.7
# verification (MSB3202 "project file not found" for all 6 projects) — fixed
# by copying each csproj explicitly instead of relying on the glob.
#
# Restoring only the API project (not the whole .sln) is also intentional:
# it transitively restores Domain/Application/Infrastructure through their
# ProjectReferences, but skips the two test projects entirely — production
# image has no reason to restore test-only packages.
COPY src/Core/Marketplace.Domain/Marketplace.Domain.csproj src/Core/Marketplace.Domain/
COPY src/Core/Marketplace.Application/Marketplace.Application.csproj src/Core/Marketplace.Application/
COPY src/Infrastructure/Marketplace.Infrastructure/Marketplace.Infrastructure.csproj src/Infrastructure/Marketplace.Infrastructure/
COPY src/Presentation/Marketplace.API/Marketplace.API.csproj src/Presentation/Marketplace.API/
RUN dotnet restore src/Presentation/Marketplace.API/Marketplace.API.csproj

COPY src/ ./src/
WORKDIR /src/src/Presentation/Marketplace.API
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Security hardening
RUN apk add --no-cache icu-libs curl \
    && addgroup -S appgroup \
    && adduser -S appuser -G appgroup -h /app

WORKDIR /app
COPY --from=build --chown=appuser:appgroup /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_ENVIRONMENT=Production

# Never bake secrets into the image — all secrets injected at runtime
# via environment variables / mounted secrets from Vault/Secret Manager.

USER appuser
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Marketplace.API.dll"]
