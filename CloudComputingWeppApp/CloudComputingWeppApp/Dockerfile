FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["CloudComputingWeppApp.csproj", "CloudComputingWeppApp/"]
RUN dotnet restore "CloudComputingWeppApp/CloudComputingWeppApp.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CloudComputingWeppApp.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudComputingWeppApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudComputingWeppApp.dll"]