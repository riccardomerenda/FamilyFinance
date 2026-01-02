# FamilyFinance Dockerfile
# Multi-stage build for optimized image size

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY FamilyFinance/FamilyFinance.csproj FamilyFinance/

# Restore dependencies
RUN dotnet restore FamilyFinance/FamilyFinance.csproj

# Copy source code
COPY FamilyFinance/ FamilyFinance/

# Build and publish
WORKDIR /src/FamilyFinance
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
# Create non-root user for security (using useradd for better compatibility)
RUN useradd -m -s /bin/bash appuser || echo "User already exists"

# Create data directory for SQLite database
RUN mkdir -p /app/data && chown -R appuser:appuser /app/data

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
# USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__Default="Data Source=/app/data/familyfinance.db"

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "FamilyFinance.dll"]

