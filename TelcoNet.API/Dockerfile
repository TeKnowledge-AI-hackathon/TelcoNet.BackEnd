# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["TelcoNet.API/TelcoNet.API.csproj", "TelcoNet.API/"]
COPY ["TelcoNet.Core/TelcoNet.Core.csproj", "TelcoNet.Core/"]
COPY ["TelcoNet.Data/TelcoNet.Data.csproj", "TelcoNet.Data/"]
COPY ["TelcoNet.Plugins/TelcoNet.Plugins.csproj", "TelcoNet.Plugins/"]

RUN dotnet restore "./TelcoNet.API/TelcoNet.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TelcoNet.API"
RUN dotnet build "./TelcoNet.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TelcoNet.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in normal mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables can be overridden at runtime
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TelcoNet.API.dll"]
