@echo off
echo Cleaning Docker cache and images...
echo.

echo Stopping all containers...
docker stop $(docker ps -aq) 2>nul

echo Removing all containers...
docker rm $(docker ps -aq) 2>nul

echo Removing all images...
docker rmi $(docker images -q) 2>nul

echo Cleaning system...
docker system prune -a -f

echo Cleaning build cache...
docker builder prune -a -f

echo Done! Docker cache cleaned.
pause

