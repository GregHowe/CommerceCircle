FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG NUGET_USER_NAME
ARG NUGET_AUTH_PASS

WORKDIR /src
COPY ["src/EventWorker/", "EventWorker/"]
COPY ["src/Application/", "Application/"]
COPY ["src/Domain/", "Domain/"]
COPY ["src/Infrastructure/", "Infrastructure/"]
COPY ["Directory.Packages.props", "/"]

RUN dotnet nuget add source https://nuget.pkg.github.com/h4b-dev/index.json -u $NUGET_USER_NAME -p $NUGET_AUTH_PASS --store-password-in-clear-text

RUN dotnet restore "EventWorker/EventWorker.csproj"
WORKDIR "/src/EventWorker"
RUN dotnet build "EventWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventWorker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS release

WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y libgdiplus

RUN useradd -r appuser
RUN chown -R appuser /app
USER appuser
ENV ASPNETCORE_URLS=http://+:8000

ENTRYPOINT ["dotnet", "N1coLoyalty.EventWorker.dll"]