# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["DoQuangThang_SE1885_A01_BE/Presentation/CoreAPI.csproj", "Presentation/"]
COPY ["DoQuangThang_SE1885_A01_BE/BusinessLogic/BusinessLogic.csproj", "BusinessLogic/"]
COPY ["DoQuangThang_SE1885_A01_BE/DataAccess/DataAccess.csproj", "DataAccess/"]

# Restore dependencies
RUN dotnet restore "Presentation/CoreAPI.csproj"

# Copy the rest of the source code
COPY DoQuangThang_SE1885_A01_BE/Presentation/ ./Presentation/
COPY DoQuangThang_SE1885_A01_BE/BusinessLogic/ ./BusinessLogic/
COPY DoQuangThang_SE1885_A01_BE/DataAccess/ ./DataAccess/

WORKDIR "/src/Presentation"
RUN dotnet build "CoreAPI.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "CoreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreAPI.dll"]
