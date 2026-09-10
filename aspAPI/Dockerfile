FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["aspAPI.csproj", "./"]
RUN dotnet restore "aspAPI.csproj"

COPY . .
RUN dotnet publish "aspAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "aspAPI.dll"]