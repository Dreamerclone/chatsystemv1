# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore "Server/Server.csproj"

# Build and publish the Server
RUN dotnet publish "Server/Server.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The port Railway or other clouds usually expect
# We will use an environment variable for the port
ENV PORT=8888
EXPOSE 8888

ENTRYPOINT ["dotnet", "Server.dll"]
