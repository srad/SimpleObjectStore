# SimpleObjectStore

SimpleObjectStore is a simple alternative Amazon S3. Is based on a server with REST API and includes an admin interface.
In order to use it you must provide and OpenID authenticator, like Keycloak.

You can either use the admin interface to manage your buckets or use the API (as the admin UI does itself).

https://github.com/srad/SimpleObjectStore/assets/1612461/809cc01c-8c96-47fa-a94d-9db2d9e458b0

# API

You can use the API to manage the buckets and files.
An [API documentation](https://htmlpreview.github.io/?https://github.com/srad/SimpleObjectStore/blob/main/SimpleObjectStore/Docs/index.html) is contained in the repo.

In order to authenticate you need to provide an valid API key which is generated for you and printed in the console, once you launched the docker image the first time.
You need to provide the header:

```
X-API-Key: <api-key>
```


# Docker setup

The docker images are available via `sedrad/simpleobjectstore-admin:1.0.0` and `sedrad/simpleobjectstore:1.0.0`.

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

The authenticated user must be in the `admin` group and the key for the role must map to `groups`.

When the container is started the first time, a default API key is created which you can see in the docker logs.
You need to specify it in the `API_KEY` key. This is used by the user interface to access the server.

# License

The license is open source and free to use for non-commercial projects and charities. You need a license to use it commercially.
