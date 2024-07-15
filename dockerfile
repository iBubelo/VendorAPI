FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VendorAPI.csproj", "."]
RUN dotnet restore "./VendorAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "VendorAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VendorAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VendorAPI.dll"]