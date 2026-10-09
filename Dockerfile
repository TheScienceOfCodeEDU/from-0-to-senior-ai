FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /source
COPY FromZeroToSeniorAI.sln global.json ./
COPY src/FromZeroToSeniorAI.Api/FromZeroToSeniorAI.Api.csproj src/FromZeroToSeniorAI.Api/
COPY tests/FromZeroToSeniorAI.Api.Tests/FromZeroToSeniorAI.Api.Tests.csproj tests/FromZeroToSeniorAI.Api.Tests/
RUN dotnet restore FromZeroToSeniorAI.sln

FROM restore AS build
COPY . .
RUN dotnet build FromZeroToSeniorAI.sln --no-restore --configuration Release

FROM build AS test
RUN dotnet test FromZeroToSeniorAI.sln --no-build --configuration Release

FROM build AS publish
RUN dotnet publish src/FromZeroToSeniorAI.Api/FromZeroToSeniorAI.Api.csproj \
    --no-build \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FromZeroToSeniorAI.Api.dll"]

