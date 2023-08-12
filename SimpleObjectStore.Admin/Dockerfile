FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

ARG TARGETARCH

WORKDIR /src
COPY ["SimpleObjectStore.Admin.csproj", "SimpleObjectStore.Admin/"]
RUN dotnet restore "SimpleObjectStore.Admin/SimpleObjectStore.Admin.csproj" -a $TARGETARCH
WORKDIR "/src/SimpleObjectStore.Admin"
COPY . .
RUN dotnet build "SimpleObjectStore.Admin.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "SimpleObjectStore.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

FROM base AS final
WORKDIR /app

ENV API__Endpoint ""
ENV API__Key ""
ENV OpenId__Authority ""
ENV OpenId__ClientId ""
ENV OpenId__ClientSecret ""

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleObjectStore.Admin.dll"]