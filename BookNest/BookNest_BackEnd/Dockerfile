# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY ../BookNest.sln ./
COPY ../BookNest_BackEnd/ ./BookNest_BackEnd/
COPY ../BookNest_Models/ ./BookNest_Models/
COPY ../BookNest_Repositories/ ./BookNest_Repositories/
COPY ../BookNest_Services/ ./BookNest_Services/

WORKDIR /src/BookNest_BackEnd
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o /app/publish

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 80

# Set environment variables (override in production)
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "BookNest_BackEnd.dll"] 