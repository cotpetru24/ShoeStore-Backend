# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project files needed for restore
COPY ShoeStore/ShoeStore.csproj ShoeStore/
COPY ShoeStore.DataContext.PostgreSQL/ShoeStore.DataContext.PostgreSQL.csproj ShoeStore.DataContext.PostgreSQL/

# Restore ONLY the API project (ignores Tests)
RUN dotnet restore ShoeStore/ShoeStore.csproj

# Copy everything else
COPY . .

# Publish API
RUN dotnet publish ShoeStore/ShoeStore.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ShoeStore.dll"]