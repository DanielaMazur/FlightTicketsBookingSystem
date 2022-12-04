#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY ./AuthService/AuthService.csproj /app/AuthService/
WORKDIR /app/AuthService
RUN dotnet restore
WORKDIR /app
COPY ./AuthService /app/AuthService/

WORKDIR /app/AuthService
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:5.0
ENV ASPNETCORE_URLS=http://+:6000  
WORKDIR /app
COPY --from=build /app/AuthService/out .
ENTRYPOINT ["/root/wait-for-it.sh", "discoveryService:9000", "-t", "120", "--", "/root/wait-for-it.sh", "sqlServer:1433","-t", "120", "--", "dotnet", "AuthService.dll"]
