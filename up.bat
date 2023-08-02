@echo off

docker compose down
docker image prune -f -a
docker compose --env-file .env up --force-recreate

exit