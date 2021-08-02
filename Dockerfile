FROM mcr.microsoft.com/dotnet/nightly/sdk:latest AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore MinimalAPI.WebApi/*.csproj
RUN dotnet publish MinimalAPI.WebApi/*.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/nightly/aspnet:latest
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "MinimalAPI.WebApi.dll"]