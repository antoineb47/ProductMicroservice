# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore src/ProductMicroservice.csproj
RUN dotnet test tests/ProductMicroservice.Tests.csproj --no-build
RUN dotnet publish src/ProductMicroservice.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
VOLUME /app/data
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "ProductMicroservice.dll"] 