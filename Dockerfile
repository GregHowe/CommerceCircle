FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG DD_GIT_REPOSITORY_URL
ARG DD_GIT_COMMIT_SHA
ENV DD_GIT_REPOSITORY_URL=${DD_GIT_REPOSITORY_URL} 
ENV DD_GIT_COMMIT_SHA=${DD_GIT_COMMIT_SHA}
ARG NUGET_USER_NAME
ARG NUGET_AUTH_PASS

WORKDIR /src
COPY ["src/Api/", "Api/"]
COPY ["src/Application/", "Application/"]
COPY ["src/Domain/", "Domain/"]
COPY ["src/Infrastructure/", "Infrastructure/"]
COPY ["Directory.Packages.props", "/"]

RUN dotnet nuget add source https://nuget.pkg.github.com/h4b-dev/index.json -u $NUGET_USER_NAME -p $NUGET_AUTH_PASS --store-password-in-clear-text

RUN dotnet restore "Api/Api.csproj"
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS release
ARG DD_GIT_REPOSITORY_URL
ARG DD_GIT_COMMIT_SHA
ENV DD_GIT_REPOSITORY_URL=${DD_GIT_REPOSITORY_URL} 
ENV DD_GIT_COMMIT_SHA=${DD_GIT_COMMIT_SHA}

WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y libgdiplus

RUN useradd -r appuser
RUN chown -R appuser /app
USER appuser
ENV ASPNETCORE_URLS=http://+:8000

ENTRYPOINT ["dotnet", "N1coLoyalty.Api.dll"]