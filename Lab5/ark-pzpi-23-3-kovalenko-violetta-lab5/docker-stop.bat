@echo off
chcp 65001 >nul
echo ========================================
echo Зупинка Docker контейнерів
echo ========================================
echo.

docker-compose down

echo.
echo Контейнери зупинено!
echo.

pause

