# ️ MISH Store | Fullstack E-Commerce Platform

> Современное SPA-приложение для интернет-магазина с акцентом на безопасность, производительность и чистую архитектуру.  
> Проект полностью задеплоен в Production через Docker на Railway.

<div align="center">
  <a href="http://mish-store-production.up.railway.app" target="_blank">
    <img src="https://img.shields.io/badge/Live-Demo-00C853?style=for-the-badge&logo=railway&logoColor=white" alt="Live Demo" />
  </a> <a/>
  <a href="https://github.com/Hass1e/mish-store" target="_blank">
    <img src="https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white" alt="GitHub Repo" />
  </a>
</div>

---

##  Технологический стек

| Область | Технологии |
| :--- | :--- |
| ⚙️ **Backend** | C#, ASP.NET Core (.NET 10), Entity Framework Core |
| 🎨 **Frontend** | React, TypeScript, React Router, CSS Modules |
| 🗄️ **Database** | PostgreSQL, SQL |
| 🐳 **DevOps** | Docker, Railway (CI/CD) |
| 🔒 **Security** | JWT (Hash+Salt), RBAC, Rate Limiting, Input Validation |
| 🧪 **Testing** | xUnit (51 юнит-тест), Moq |
| 🛠️ **Tools** | Swagger/OpenAPI, Git (Branching, Tags, PR) |

---

## ✨ Ключевые особенности

### 🔐 Безопасность и аутентификация
- **JWT Authentication:** Стандартная реализация через `AddJwtBearer`. Пароли хешируются алгоритмом **BCrypt** перед сохранением в БД.
- **RBAC (Role-Based Access Control):** Разграничение прав доступа через политики `[Authorize(Roles = "...")]` на уровне контроллеров и эндпоинтов.
- **Rate Limiting:** Встроенный middleware `AddRateLimiter` для защиты от брутфорса и DDoS.
- **Валидация данных:** Data Annotations + FluentValidation для предотвращения инъекций и некорректных входных данных.

###  Архитектура Backend
- **Layered Architecture:** Controllers → Services → Repositories. Бизнес-логика изолирована от инфраструктуры.
- **DTO Pattern:** Разделение моделей (`CreateDto`, `ViewDto`, `UpdateDto`) для типобезопасности.
- **Async/Await:** Полностью неблокирующий I/O через `NpgsqlConnection`.
- **Config Management:** Чувствительные данные вынесены в `.env` / User Secrets. Настройки централизованы в `appsettings.json`.

###  Frontend и UX
- **SPA Роутинг:** Навигация без перезагрузки страницы с защитой приватных роутов.
- **Responsive Design:** Адаптив (Mobile/Tablet/Desktop) через CSS Media Queries и `max-width`.
- **Performance:** Skeleton Loaders, оптимизированные API-запросы.
- **Media Optimization:** Автоконвертация изображений в JPG (80% качество) при загрузке.

###  Тестирование и качество
- **51 юнит-тест:** Покрытие сервисов, валидации DTO и permission-логики.
- **AI-Assisted Development:** Все архитектурные решения принимаются самостоятельно, каждый AI-generated фрагмент проходит строгий code review.

### 🗄 База данных и DevOps
- **Миграция:** Перенос схемы с SQLite на PostgreSQL (9 таблиц, связи, индексы).
- **Seeding:** Автонаполнение БД тестовыми данными + создание админа (пароль генерируется случайно, если не задан в `.env`).
- **Dockerization:** Контейнеризация + автодеплой на Railway.
- **Swagger UI:** Интерактивная документация API.

---
## 📂 Структура проекта

```
mish-store/
├── 📁 Mish.Api/            # Backend: Controllers, Services, Repositories, DTOs
├── 📁 mish-client/         # Frontend: React components, pages, hooks, utils
├── 🐳 docker-compose.yml   # Оркестрация контейнеров для локального запуска
└── 📄 README.md            # Документация проекта
```
##  Локальный запуск

<details>
<summary><b> Click me </b></summary>

### Требования
- .NET SDK 10.0+
- PostgreSQL (или Docker-образ из репозитория)

## ⚙️ Конфигурация окружения

Создай файл `.env` в корне проекта на основе шаблона ниже.  
**️ Никогда не коммить этот файл в Git!** Убедись, что он добавлен в `.gitignore`.

```bash
# Секретный ключ для JWT (минимум 32 символа, используй генератор случайных строк)
JwtSettings__SecretKey= [YOUR_GENERATED_SECRET_KEY_HERE_MIN_32_CHARS]

# Время жизни токена (в минутах)
JwtSettings__AccessTokenExpirationMinutes=4320

# Настройки куки
AuthCookie__ExpirationMinutes=4320

# Пароль админа при первом запуске 
# Если оставить пустым — система сгенерирует случайный пароль и выведет его в консоль
SeedAdmin__Password=

# URL API (для локальной разработки)
ApiSettings__BaseUrl=http://localhost:5073

# Пароль PostgreSQL (придумай свой сложный пароль)
DB_PASSWORD= [YOUR_STRONG_DB_PASSWORD_HERE]

# Строка подключения к БД
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=mish_db;Username=postgres;Password=[YOUR_STRONG_DB_PASSWORD_HERE]
```

### Backend
```bash
cd Mish.Api
dotnet restore
dotnet ef database update
dotnet run --launch-profile "Development"
# Swagger: https://localhost:5073/swagger

### Frontend
cd mish-client
npm install
npm run dev
# Приложение доступно по адресу: http://localhost:5173

### Docker (рекомендуемый способ)
docker-compose up --build -d

```
</details>

## 👤 Автор

**Александр**  
Fullstack Developer (C# / ASP.NET Core / React)

-📱 [Telegram](https://t.me/ES_Deuterium) 

