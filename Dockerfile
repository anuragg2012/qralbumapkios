# Stage 1 - Build the Angular frontend
FROM node:20 AS frontend-builder
WORKDIR /app

# Install dependencies
COPY frontend/mobile/package*.json frontend/mobile/
RUN npm ci --prefix frontend/mobile

# Build the Angular project
COPY frontend/mobile frontend/mobile
RUN mkdir -p backend/api/wwwroot \
    && npm run build --prefix frontend/mobile

# Stage 2 - Build and publish the .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-builder
WORKDIR /src

COPY backend ./backend
COPY --from=frontend-builder /app/backend/api/wwwroot ./backend/api/wwwroot
RUN dotnet restore backend/api/QRAlbums.API.csproj
RUN dotnet publish backend/api/QRAlbums.API.csproj -c Release -o /app/publish

# Stage 3 - Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=backend-builder /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "QRAlbums.API.dll"]
