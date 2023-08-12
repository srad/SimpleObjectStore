FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

ARG TARGETARCH

WORKDIR /src
COPY ["SimpleObjectStore.csproj", "SimpleObjectStore/"]
RUN dotnet restore "SimpleObjectStore/SimpleObjectStore.csproj" -a $TARGETARCH
WORKDIR "/src/SimpleObjectStore"
COPY . .
RUN dotnet build "SimpleObjectStore.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "SimpleObjectStore.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

FROM base AS final
WORKDIR /app
ENV STORAGE_DIRECTORY=/data/storage
ENV DB_PATH=/data/data.db
RUN mkdir -p $STORAGE_DIRECTORY
CMD /bin/sh -c "echo Storing file in: $STORAGE_DIRECTORY. Storing data in: $DB_PATH"
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleObjectStore.dll"]