FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY SigneWordBotAspCore/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/SigneWordBotAspCore/out .
# ENTRYPOINT ["dotnet", "aspnetapp.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet SigneWordBotAspCore.dll
