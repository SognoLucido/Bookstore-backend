#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bookstore-backend/Bookstore-backend.csproj", "Bookstore-backend/"]
COPY ["Auth/Auth.csproj", "Auth/"]
COPY ["Database/Database.csproj", "Database/"]
RUN dotnet restore "./Bookstore-backend/Bookstore-backend.csproj"
COPY . .
WORKDIR "/src/Bookstore-backend"
RUN dotnet build "./Bookstore-backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bookstore-backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookstore-backend.dll"]