FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["./RescueRS/ResgateRS.csproj", "RescueRS/"]
RUN dotnet restore "RescueRS/ResgateRS.csproj"
COPY . .
WORKDIR "/src/RescueRS"
RUN dotnet build "ResgateRS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ResgateRS.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResgateRS.dll"]