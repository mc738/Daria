FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Daria.App/Daria.App.fsproj", "Daria.App/"]
RUN dotnet restore "Daria.App/Daria.App.fsproj"
COPY . .
WORKDIR "/src/Daria.App"
RUN dotnet build "Daria.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Daria.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Daria.App.dll"]
