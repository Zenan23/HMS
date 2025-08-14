# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY backend/eBooking/API/API.csproj backend/eBooking/API/
RUN dotnet restore backend/eBooking/API/API.csproj

# copy source and publish
COPY backend/eBooking/API/ backend/eBooking/API/
WORKDIR /src/backend/eBooking/API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]
