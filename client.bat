@echo off

if not exist ".\Resources\bin" (
    md .\Resources\bin
)

if not exist ".\Resources\bin\nswag" (
    echo "Downloading nswag ..."
    curl.exe  --output .\Resources\bin\nswag.zip -L https://github.com/RicoSuter/NSwag/releases/download/v13.19.0/NSwag.zip
    md .\Resources\bin\nswag
    tar -xf .\Resources\bin\nswag.zip -C .\Resources\bin\nswag
    del .\Resources\bin\nswag.zip
)

.\Resources\bin\nswag\Net70\dotnet-nswag.exe swagger2csclient  /input:https://localhost:44312/swagger/v1/swagger.json /classname:SimpleObjectStoreClient /namespace:SimpleObjectStore.Services.v1 /output:.\SimpleObjectStore.Admin\Services\v1\StorageClient.cs  /ArrayType:System.Collections.Generic.List /JsonLibrary:SystemTextJson

exit