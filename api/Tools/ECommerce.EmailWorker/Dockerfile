# syntax=docker/dockerfile:1
#
# Build context MUST be src/api (not this directory) because ECommerce.EmailWorker references
# ../../Shared/ECommerce.Shared — the whole src/api tree needs to be visible to
# `dotnet restore`/`dotnet publish`.
#
#   docker build -f src/api/Tools/ECommerce.EmailWorker/Dockerfile src/api
#
# (docker-compose.yml at the repo root wires this up with the correct context.)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire src/api tree (Tools/EmailWorker + Shared) so project references resolve.
COPY . .

RUN dotnet restore Tools/ECommerce.EmailWorker/ECommerce.EmailWorker.csproj
RUN dotnet publish Tools/ECommerce.EmailWorker/ECommerce.EmailWorker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Background worker only (RabbitMQ consumer + SQLite-backed inbox) — no HTTP surface, no EXPOSE.
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ECommerce.EmailWorker.dll"]
