# Stage 1: build the app with the .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish SchoolAdmission.Api/SchoolAdmission.Api.csproj -c Release -o /app

# Stage 2 (only for tests): reuses the SDK stage and runs the test project
FROM build AS test
ENTRYPOINT ["dotnet", "test", "SchoolAdmission.Tests/SchoolAdmission.Tests.csproj"]

# Stage 3: run the app with the small .NET 10 runtime (this is the default image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SchoolAdmission.Api.dll"]