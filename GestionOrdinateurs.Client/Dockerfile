# =========================
# Build
# =========================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
COPY GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj ./GestionOrdinateurs.Client/
RUN dotnet restore ./GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj

# Copy the remaining source code
COPY GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
COPY GestionOrdinateurs.Client/ ./GestionOrdinateurs.Client/
RUN dotnet publish ./GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj -c Release -o /app/publish

# Special case for the nginx file, to make it run on docker
FROM nginx:alpine AS runtime
COPY GestionOrdinateurs.Client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot/ /usr/share/nginx/html/
EXPOSE 5159
