FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["DirectoryService/.editorconfig", "./DirectoryService/"]

RUN dotnet nuget update source github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text \
    --configfile nuget.config

COPY ["DirectoryService/", "DirectoryService/"]

RUN dotnet restore "DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj"

WORKDIR "/src/DirectoryService/src/DirectoryService.Web"

RUN dotnet publish "DirectoryService.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5051
ENTRYPOINT ["dotnet", "DirectoryService.Web.dll"]
