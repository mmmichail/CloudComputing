FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CloudComputing.csproj", "CloudComputing/"]
RUN dotnet restore "CloudComputing/CloudComputing.csproj"
COPY . .

# Ensure the /app directory has the correct permissions before building
RUN mkdir -p /app/build && chown -R $(whoami) /app/build && chmod -R 777 /app/build

WORKDIR "/src"
RUN dotnet build "CloudComputing.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
RUN dotnet publish "CloudComputing.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudComputing.dll"]