# SimpleObjectStore

 [![Build Status](http://teamcity.sedrad.com/app/rest/builds/buildType:(id:SimpleObjectStore_Build)/statusIcon)](http://teamcity/viewType.html?buildTypeId=SimpleObjectStore_Build&guest=1)

SimpleObjectStore is a simple alternative Amazon S3. Is based on a server with REST API and includes an admin interface.
In order to use it you must provide and OpenID authenticator, like Keycloak.

You can either use the admin interface to manage your buckets or use the API (as the admin UI does itself).

https://github.com/srad/SimpleObjectStore/assets/1612461/0cfd755b-c351-434e-8272-6624c4333006

# API

You can use the API to manage the buckets and files.
An [API documentation](https://htmlpreview.github.io/?https://github.com/srad/SimpleObjectStore/blob/main/SimpleObjectStore/Docs/v1/index.html) is contained in the repo.

In order to authenticate you need to provide an valid API key which is generated for you and printed in the console, once you launched the docker image the first time.
You need to provide the header:

```
X-API-Key: <api-key>
```


# Docker setup

The docker images are available via [`sedrad/simpleobjectstore-admin`](https://hub.docker.com/repository/docker/sedrad/simpleobjectstore-admin/general) and [`sedrad/simpleobjectstore`](https://hub.docker.com/repository/docker/sedrad/simpleobjectstore/general).

The included `docker-compose.yml` shows what you need to run the containers.

These are example values for local use. Assuming you have, i.e. Keycloak runnig in a local container on port 9000 and the SimpleObjectStore server on port 5000:

```
  environment:
    - API__Endpoint=http://host.docker.internal:5000
    - API__Key=123456
    - OpenId__Authority=http://host.docker.internal:9000/realms/master
    - OpenId__ClientId=objectstore-client
    - OpenId__ClientSecret=654321
```

These values will be taken if the `appsettings.json` configuration entries are missing:

```
{
  "OpenId": {
    "Authority": "http://host.docker.internal:9000/realms/master",
    "ClientId": "objectstore-client",
    "ClientSecret": "654321"
  },
  "API": {
    "Endpoint": "http://host.docker.internal:5000",
    "Key": "123456"
  }
}
```

## Openid

The claim name for the OpenId authorization is `roles` which must contain the role `objectstore` to gain access to the administration interface.

When the container is started the first time a default API key is created and written to the console, which you can see in the docker logs.
You need to specify it in the `API_KEY` environment variable. This is used by the administration interface to access the API and it could be used by any other client to access the SimpleObjectStore.

# License

The license is open source and free to use for non-commercial projects and charities. You need a license to use it commercially.
