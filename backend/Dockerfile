FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything and restore as distinct layers for better build caching
COPY . .
RUN dotnet restore src/Host/Resit.Api/Resit.Api.csproj

RUN dotnet publish src/Host/Resit.Api/Resit.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Matches ReceiptStorage:RootPath in appsettings.json. Mount this as a persistent volume
# in Coolify (or docker-compose) so receipt files survive redeploys and container restarts.
VOLUME ["/app/storage"]

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Resit.Api.dll"]
