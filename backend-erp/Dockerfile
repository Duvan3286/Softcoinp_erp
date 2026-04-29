# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["Softcoinp.ERP.Domain/Softcoinp.ERP.Domain.csproj", "Softcoinp.ERP.Domain/"]
COPY ["Softcoinp.ERP.Application/Softcoinp.ERP.Application.csproj", "Softcoinp.ERP.Application/"]
COPY ["Softcoinp.ERP.Infrastructure/Softcoinp.ERP.Infrastructure.csproj", "Softcoinp.ERP.Infrastructure/"]
COPY ["Softcoinp.ERP.WebAPI/Softcoinp.ERP.WebAPI.csproj", "Softcoinp.ERP.WebAPI/"]

RUN dotnet restore "Softcoinp.ERP.WebAPI/Softcoinp.ERP.WebAPI.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Softcoinp.ERP.WebAPI"
RUN dotnet build "Softcoinp.ERP.WebAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Softcoinp.ERP.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Softcoinp.ERP.WebAPI.dll"]
