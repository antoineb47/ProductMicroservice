# Base image for both dev and prod
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ProductMicroservice.csproj", "src/"]
RUN dotnet restore "src/ProductMicroservice.csproj"
COPY . .
RUN dotnet build "src/ProductMicroservice.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR /
RUN dotnet test --no-restore -c Release

FROM build AS publish
RUN dotnet publish "src/ProductMicroservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME /app/data
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ProductMicroservice.dll"] 