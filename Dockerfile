# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Build the Server project
RUN dotnet publish "Server/Server.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway dynamic port
ENV PORT=8888
EXPOSE 8888

ENTRYPOINT ["dotnet", "Server.dll"]
