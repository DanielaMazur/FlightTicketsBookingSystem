#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY ./CacheService/CacheService.csproj /app/CacheService/
WORKDIR /app/CacheService
RUN dotnet restore
WORKDIR /app
COPY ./CacheService /app/CacheService/

WORKDIR /app/CacheService
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:5.0
ENV ASPNETCORE_URLS=http://+:7000
WORKDIR /app
COPY --from=build /app/CacheService/out .
ENTRYPOINT ["dotnet", "CacheService.dll"]