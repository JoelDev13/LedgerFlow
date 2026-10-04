FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so restore layer is cached
COPY LedgerFlow.sln ./
COPY LedgerFlow.Domain/LedgerFlow.Domain.csproj             LedgerFlow.Domain/
COPY LedgerFlow.Application/LedgerFlow.Application.csproj   LedgerFlow.Application/
COPY LedgerFlow.Infrastructure/LedgerFlow.Infrastructure.csproj LedgerFlow.Infrastructure/
COPY LedgerFlow.Api/LedgerFlow.Api.csproj                   LedgerFlow.Api/

RUN dotnet restore LedgerFlow.sln

# Copy source and publish
COPY LedgerFlow.Domain/         LedgerFlow.Domain/
COPY LedgerFlow.Application/    LedgerFlow.Application/
COPY LedgerFlow.Infrastructure/ LedgerFlow.Infrastructure/
COPY LedgerFlow.Api/            LedgerFlow.Api/

RUN dotnet publish LedgerFlow.Api/LedgerFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl needed for HEALTHCHECK
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Non root user UID 1001
RUN groupadd --gid 1001 appgroup \
    && useradd --uid 1001 --gid appgroup --shell /bin/false --no-create-home appuser

COPY --from=build --chown=appuser:appgroup /app/publish ./

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl --fail http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "LedgerFlow.Api.dll"]
