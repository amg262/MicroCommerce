FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Micro.GatewaySolution/Micro.GatewaySolution.csproj", "Micro.GatewaySolution/"]
RUN dotnet restore "Micro.GatewaySolution/Micro.GatewaySolution.csproj"
COPY . .
WORKDIR "/src/Micro.GatewaySolution"
RUN dotnet build "Micro.GatewaySolution.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Micro.GatewaySolution.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Micro.GatewaySolution.dll"]
