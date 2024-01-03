FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
#EXPOSE 443

ARG GitHub-Username
ARG GitHub-APIKEY

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TraefikApi/TraefikApi.csproj", "TraefikApi/"]
RUN dotnet restore "TraefikApi/TraefikApi.csproj" --ignore-failed-sources --source "https://api.nuget.org/v3/index.json" --source "https://nuget.pkg.github.com/${GitHub-Username}/index.json" --username ${GitHub-Username} --password ${GitHub-APIKEY} 
COPY . .
WORKDIR "/src/TraefikApi"
RUN dotnet build "TraefikApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TraefikApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TraefikApi.dll"]
