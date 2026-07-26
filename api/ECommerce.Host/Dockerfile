# syntax=docker/dockerfile:1
#
# Build context MUST be src/api (not this directory) because ECommerce.Host references
# ../Shared/ECommerce.Shared and ../Modules/*/ECommerce.Modules.* — the whole src/api tree
# needs to be visible to `dotnet restore`/`dotnet publish`.
#
#   docker build -f src/api/ECommerce.Host/Dockerfile src/api
#
# (docker-compose.yml at the repo root wires this up with the correct context.)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire src/api tree (Host + Shared + all Modules) so project references resolve.
COPY . .

RUN dotnet restore ECommerce.Host/ECommerce.Host.csproj
RUN dotnet publish ECommerce.Host/ECommerce.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Kestrel listens on 8080 inside the container; map/reverse-proxy as needed on the host.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ECommerce.Host.dll"]
