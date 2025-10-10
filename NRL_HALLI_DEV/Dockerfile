FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NRL_HALLI_DEV/NRL_HALLI_DEV.csproj", "NRL_HALLI_DEV/"]
RUN dotnet restore "NRL_HALLI_DEV/NRL_HALLI_DEV.csproj"
COPY . .
WORKDIR "/src/NRL_HALLI_DEV"
RUN dotnet build "./NRL_HALLI_DEV.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NRL_HALLI_DEV.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NRL_HALLI_DEV.dll"]
