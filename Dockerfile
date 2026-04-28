# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["TelcoNet.API/TelcoNet.API.csproj", "TelcoNet.API/"]
COPY ["TelcoNet.Core/TelcoNet.Core.csproj", "TelcoNet.Core/"]
COPY ["TelcoNet.Data/TelcoNet.Data.csproj", "TelcoNet.Data/"]
COPY ["TelcoNet.Plugins/TelcoNet.Plugins.csproj", "TelcoNet.Plugins/"]

RUN dotnet restore "./TelcoNet.API/TelcoNet.API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/TelcoNet.API"
RUN dotnet build "./TelcoNet.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TelcoNet.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelcoNet.API.dll"]
