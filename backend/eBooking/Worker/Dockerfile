# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj files first for better layer caching
COPY ["Worker/Worker.csproj", "Worker/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Persistence/Persistence.csproj", "Persistence/"]

# restore dependencies
RUN dotnet restore "Worker/Worker.csproj"

# copy source code
COPY . .

# build and publish in one step to reduce layers
WORKDIR "/src/Worker"
RUN dotnet build "Worker.csproj" -c Release -o /app/build && \
    dotnet publish "Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final stage (Worker je Generic Host, ne treba ASP.NET Core runtime — samo dotnet/runtime)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Worker.dll"]
