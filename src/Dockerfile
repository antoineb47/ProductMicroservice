# Use the .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /solution

# Copy solution files
COPY . .

# Restore dependencies
RUN dotnet restore src/ProductMicroservice.csproj

# Build and publish
RUN dotnet publish src/ProductMicroservice.csproj -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET Core runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./

# Create directories
RUN mkdir -p /app/data

# Set environment variables
ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT
ENV ASPNETCORE_URLS=http://+:80

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
    CMD wget -qO- http://localhost:80/api/product || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "ProductMicroservice.dll"] 