@echo off

docker build -t sedrad/simpleobjectstore:latest .\SimpleObjectStore
docker push sedrad/simpleobjectstore

docker build -t sedrad/simpleobjectstore-admin:latest .\SimpleObjectStore.Admin
docker push sedrad/simpleobjectstore-admin