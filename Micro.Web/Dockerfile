FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Micro.Web/Micro.Web.csproj", "Micro.Web/"]
RUN dotnet restore "Micro.Web/Micro.Web.csproj"
COPY . .
WORKDIR "/src/Micro.Web"
RUN dotnet build "Micro.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Micro.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Micro.Web.dll"]
