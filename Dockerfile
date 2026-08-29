# -------------------------------------------------------------
# Stage 1: Build React Frontend
# -------------------------------------------------------------
FROM node:20-alpine AS build-frontend
WORKDIR /app/frontend

# Copy package manifests and install dependencies
COPY frontend/package*.json ./
RUN npm ci

# Copy frontend source files and build static bundle
COPY frontend/ ./
RUN npm run build

# -------------------------------------------------------------
# Stage 2: Build C# ASP.NET Core Backend (.NET 10.0)
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-backend
WORKDIR /src/FikaWebApp

# Copy project file and restore NuGet dependencies
COPY FikaWebApp/FikaWebApp.csproj ./
RUN dotnet restore "FikaWebApp.csproj"

# Copy full backend source code into working directory
COPY FikaWebApp/ ./

# Copy built static assets from Stage 1 directly into wwwroot
COPY --from=build-frontend /app/frontend/dist ./wwwroot

# Publish ASP.NET Core application in Release mode
RUN dotnet publish "FikaWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------------------------------------------
# Stage 3: Final Production Runtime Environment (.NET 10.0)
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build-backend /app/publish .
ENTRYPOINT ["dotnet", "FikaWebApp.dll"]