# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ../GestionOrdinateurs/AuthentificationApi.csproj ./AuthentificationApi/
RUN dotnet restore ./AuthentificationApi

# Copy the remaining source code
COPY ./GestionOrdinateurs.Gateway/ ./GestionOrdinateurs.Gateway/
RUN dotnet publish ./GestionOrdinateurs.Gateway -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set environment variables if needed (e.g., ASPNETCORE_ENVIRONMENT)
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port 5160
EXPOSE 5160

# Define the entry point
ENTRYPOINT ["dotnet", "GestionOrdinateurs.Gateway.dll"]
