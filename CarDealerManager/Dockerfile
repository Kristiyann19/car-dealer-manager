FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["CarDealerManager/CarDealerManager.WebAPI.csproj", "CarDealerManager/"]
COPY ["CarDealerManager.Application/CarDealerManager.Application.csproj", "CarDealerManager.Application/"]
COPY ["CarDealerManager.Domain/CarDealerManager.Domain.csproj", "CarDealerManager.Domain/"]
COPY ["CarDealerManager.Infrastructure/CarDealerManager.Infrastructure.csproj", "CarDealerManager.Infrastructure/"]

RUN dotnet restore "CarDealerManager/CarDealerManager.WebAPI.csproj"

COPY . .

RUN dotnet publish "CarDealerManager/CarDealerManager.WebAPI.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CarDealerManager.WebAPI.dll"]
