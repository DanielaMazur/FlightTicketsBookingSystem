#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY ./CustomerService/CustomerService.csproj /app/CustomerService/
WORKDIR /app/CustomerService
RUN dotnet restore
WORKDIR /app
COPY ./CustomerService /app/CustomerService/

WORKDIR /app/CustomerService
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:5.0
ENV ASPNETCORE_URLS=http://+:3000  
WORKDIR /app
COPY --from=build /app/CustomerService/out .
ENTRYPOINT ["/root/wait-for-it.sh", "discoveryService:9000", "-t", "120", "--", "dotnet", "CustomerService.dll"]
