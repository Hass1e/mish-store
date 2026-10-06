# --- ЭТАП 1: Сборка React (Vite) ---
FROM node:20-alpine AS frontend-build
WORKDIR /src/mish.web

# Копируем ТОЛЬКО package.json из папки mish.web
COPY mish.web/package*.json ./
RUN npm install

# Копируем весь исходник фронта и билдим
COPY mish.web/ .

RUN npx vite build --config vite.config.prod.ts

# --- ЭТАП 2: Сборка ASP.NET Core                        (ИСПРАВЛЕНО НА .NET 10) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /src

# Восстанавливаем NuGet пакеты
COPY Mish.Api/Mish.Api.csproj Mish.Api/
RUN dotnet restore Mish.Api/Mish.Api.csproj

# Копируем весь проект (кроме того, что в .dockerignore)
COPY . .

# МАГИЯ: Берем собранный фронтенд из этапа 1 и кладем в wwwroot бэкенда
COPY --from=frontend-build /src/mish.web/dist ./Mish.Api/wwwroot

WORKDIR /src/Mish.Api

 # Ставим некомерческую лицензию для SixLabors.ImageSharp.Web 
ENV SIXLABORS_LICENSE_TYPE=NonCommercial


RUN dotnet publish -c Release -o /app/publish 

# --- ЭТАП 3: Финальный легкий образ                        (ИСПРАВЛЕНО НА .NET 10) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Копируем только скомпилированный бэкенд + статику
COPY --from=backend-build /app/publish .

# Настраиваем порт
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "Mish.Api.dll"]