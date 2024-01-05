FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Micro.Services.CouponAPI/Micro.Services.CouponAPI.csproj", "Micro.Services.CouponAPI/"]
RUN dotnet restore "Micro.Services.CouponAPI/Micro.Services.CouponAPI.csproj"
COPY . .
WORKDIR "/src/Micro.Services.CouponAPI"
RUN dotnet build "Micro.Services.CouponAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Micro.Services.CouponAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Micro.Services.CouponAPI.dll"]
