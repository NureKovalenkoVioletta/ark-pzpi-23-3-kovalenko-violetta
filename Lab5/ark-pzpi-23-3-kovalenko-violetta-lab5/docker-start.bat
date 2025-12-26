@echo off
chcp 65001 >nul
echo ========================================
echo Docker Setup - Fitness Project
echo ========================================
echo.

echo Перевірка Docker...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ПОМИЛКА: Docker не встановлено або не запущено!
    echo.
    echo Встановіть Docker Desktop з: https://www.docker.com/products/docker-desktop/
    echo Переконайтесь, що Docker Desktop запущений.
    pause
    exit /b 1
)

echo Docker знайдено!
echo.

echo Перевірка Docker Compose...
docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ПОМИЛКА: Docker Compose не знайдено!
    pause
    exit /b 1
)

echo Docker Compose знайдено!
echo.

echo ========================================
echo Запуск системи...
echo ========================================
echo.

echo Команда: docker-compose up --build -d
echo.
echo Це може зайняти кілька хвилин при першому запуску.
echo.

docker-compose up --build -d

echo.
echo Docker контейнери запущені в фоновому режимі.
echo.
echo Очікування запуску IoT клієнта в Docker...
echo.

echo.
echo ========================================
echo Система запущена!
echo.
echo Docker контейнери працюють у фоновому режимі
echo.
echo Доступ до сервісів:
echo   - Swagger API: http://localhost:5006/swagger
echo   - IoT Client (Web): http://localhost:5000
echo.
echo Дочекайтесь 30-60 секунд для повного запуску.
echo ========================================
echo.

pause

