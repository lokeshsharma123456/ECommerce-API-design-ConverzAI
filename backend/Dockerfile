# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj first for better layer caching on restore
COPY src/EcommerceAPI/EcommerceAPI.csproj src/EcommerceAPI/
RUN dotnet restore src/EcommerceAPI/EcommerceAPI.csproj

# Copy the rest and publish
COPY src/ src/
RUN dotnet publish src/EcommerceAPI/EcommerceAPI.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]
