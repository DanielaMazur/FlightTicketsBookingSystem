#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY ./AdminService/AdminService.csproj /app/AdminService/
WORKDIR /app/AdminService
RUN dotnet restore
WORKDIR /app
COPY ./AdminService /app/AdminService/

WORKDIR /app/AdminService
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:5.0
ENV ASPNETCORE_URLS=http://+:4000  
WORKDIR /app
COPY --from=build /app/AdminService/out .
ENTRYPOINT ["dotnet", "AdminService.dll"]