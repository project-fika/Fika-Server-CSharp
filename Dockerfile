# -------------------------------------------------------------
# Stage 1: Build React Frontend
# -------------------------------------------------------------
FROM node:20-alpine AS build-frontend
WORKDIR /app/frontend

COPY frontend/package*.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

# -------------------------------------------------------------
# Stage 2: Build C# ASP.NET Core Backend (.NET 10.0)
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-backend
WORKDIR /src

# Set environment flag to disable MSBuild npm target
ENV BuildingInsideDocker=true

# Restore NuGet dependencies
COPY FikaShared/FikaShared.csproj ./FikaShared/
COPY FikaWebApp/FikaWebApp.csproj ./FikaWebApp/
RUN dotnet restore "FikaWebApp/FikaWebApp.csproj"

# Copy source code
COPY FikaShared/ ./FikaShared/
COPY FikaWebApp/ ./FikaWebApp/

WORKDIR /src/FikaWebApp
RUN dotnet publish "FikaWebApp.csproj" -c Release -o /app/publish -p:UseAppHost=true

# Copy React static build output directly into the published wwwroot directory
COPY --from=build-frontend /app/frontend/dist /app/publish/wwwroot

# -------------------------------------------------------------
# Stage 3: Final Production Runtime Environment (.NET 10.0)
# -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build-backend /app/publish .
ENTRYPOINT ["dotnet", "FikaWebApp.dll"]