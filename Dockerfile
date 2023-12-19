FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LeagueLobby/LeagueLobby.csproj", "LeagueLobby/"]
RUN dotnet restore "LeagueLobby/LeagueLobby.csproj"
COPY . .
WORKDIR "/src/LeagueLobby"
RUN dotnet build "LeagueLobby.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeagueLobby.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeagueLobby.dll"]
