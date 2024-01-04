﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
#EXPOSE 443
ARG GITHUB_TOKEN

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TraefikApi/TraefikApi.csproj", "TraefikApi/"]
COPY "TraefikApi/nuget.config" "TraefikApi/"
#RUN dotnet nuget add source "https://nuget.pkg.github.com/EmilZackrisson/index.json" --name "GitHub" --username EmilZackrisson --password $GITHUB_TOKEN --store-password-in-clear-text

# Replace USERNAME and TOKEN in nuget.config with the actual values
RUN sed -i 's/USERNAME/EmilZackrisson/g' nuget.config
RUN sed -i 's/TOKEN/'"$GITHUB_TOKEN"'/g' nuget.config

RUN dotnet restore
COPY . .
WORKDIR "/src/TraefikApi"
RUN dotnet build "TraefikApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TraefikApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TraefikApi.dll"]
