FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Micro.Services.AuthAPI/Micro.Services.AuthAPI.csproj", "Micro.Services.AuthAPI/"]
RUN dotnet restore "Micro.Services.AuthAPI/Micro.Services.AuthAPI.csproj"
COPY . .
WORKDIR "/src/Micro.Services.AuthAPI"
RUN dotnet build "Micro.Services.AuthAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Micro.Services.AuthAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Micro.Services.AuthAPI.dll"]
