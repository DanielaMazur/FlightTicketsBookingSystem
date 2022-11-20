#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY ./DicoveryService/DicoveryService.csproj /app/DicoveryService/
WORKDIR /app/DicoveryService
RUN dotnet restore
WORKDIR /app
COPY ./DicoveryService /app/DicoveryService/

WORKDIR /app/DicoveryService
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:9000  
COPY --from=build /app/DicoveryService/out .
ENTRYPOINT ["dotnet", "DicoveryService.dll"]