FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Directory.Build.props ./
COPY Directory.Build.targets ./
COPY Directory.Packages.props ./
COPY .editorconfig ./
COPY src/ ./src/

RUN dotnet restore src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj
RUN dotnet publish src/HC.LIS/HC.LIS.API/HC.LIS.API.csproj \
    -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HC.LIS.API.dll"]
