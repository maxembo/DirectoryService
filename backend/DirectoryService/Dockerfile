FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG NUGET_USERNAME
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["DirectoryService/.editorconfig", "./DirectoryService/"]
COPY ["DirectoryService/", "DirectoryService/"]

RUN --mount=type=secret,id=nuget_password,required=true \
      NuGetPackageSourceCredentials_github="Username=${NUGET_USERNAME};Password=$(cat /run/secrets/nuget_password);ValidAuthenticationTypes=Basic" \
      dotnet restore \
        "DirectoryService/src/DirectoryService.Web/DirectoryService.Web.csproj" \
        --configfile /src/nuget.config


WORKDIR "/src/DirectoryService/src/DirectoryService.Web"

RUN dotnet publish "DirectoryService.Web.csproj" \
      -c Release \
      -o /app/publish \
      /p:UseAppHost=false \
      --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5051
ENTRYPOINT ["dotnet", "DirectoryService.Web.dll"]
