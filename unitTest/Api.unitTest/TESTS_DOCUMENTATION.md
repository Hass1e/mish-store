# Документация по unit-тестам Mish.Api

В таблицах перечислены тестовые методы. Методы с `[Theory]` запускаются отдельно для каждой строки `[InlineData]`.

## BruteForceProtectionTests.cs (5 методов, 8 запусков)

| # | Название теста | Описание |
|---|----------------|----------|
| 1 | `Successful_Login_Does_Not_Increment_Counters` | Успешный вход не увеличивает счётчики неудачных попыток |
| 2 | `Failed_Login_Increments_All_Counters` | Неудачный вход увеличивает счётчики `_min`, `_hour` и `_day` |
| 3 | `NonUnauthorized_Response_Does_Not_Increment_Counters` | Ответы 400 и 500 не увеличивают счётчики |
| 4 | `Fifth_Failed_Login_Is_Allowed_And_Next_Login_Is_Rejected` | Пять неудачных попыток разрешены, следующая блокируется с кодом 429 |
| 5 | `Reaching_Any_Limit_Rejects_Next_Login` | Достижение любого лимита — 5 в минуту, 10 в час или 15 в день — блокирует следующий вход |

## CategoriesControllerTests.cs (2 метода, 2 запуска)

| # | Название теста | Описание |
|---|----------------|----------|
| 6 | `GetTree_ShouldReturnOkResult_WithTreeFromService` | Возвращает HTTP 200 с деревом категорий, полученным от сервиса |
| 7 | `GetTree_ShouldReturnEmptyList_WhenServiceReturnsEmpty` | Возвращает пустой список, когда сервис не находит категории |

## CategoriesServiceTests.cs (5 методов, 5 запусков)

| # | Название теста | Описание |
|---|----------------|----------|
| 8 | `GetTreeAsync_WithStandardHierarchy_ShouldBuildCorrectTree` | Строит дерево категорий с корневыми элементами и дочерними категориями |
| 9 | `GetTreeAsync_WithEmptyDatabase_ShouldReturnEmptyList` | Возвращает пустой список для пустой базы данных |
| 10 | `GetTreeAsync_WithDeepNesting_ShouldBuildFullHierarchy` | Строит дерево с глубокой вложенностью |
| 11 | `GetTreeAsync_ShouldRespectNameOrdering` | Сортирует категории по имени |
| 12 | `GetTreeAsync_WithOrphanedCategories_ShouldNotCrashAndIgnoreOrphans` | Не падает и не включает категории с несуществующим родителем |

## OrderServiceTests.cs (2 метода, 9 запусков)

| # | Название теста | Описание |
|---|----------------|----------|
| 13 | `IsStatusTransitionAllowed_ShouldValidateCorrectly` | Проверяет допустимые и недопустимые переходы между статусами заказа для восьми наборов данных |
| 14 | `IsStatusTransitionAllowed_SameStatus_ShouldReturnTrue` | Разрешает переход в тот же статус |

## PermissionTests.cs (3 метода, 5 запусков)

| # | Название теста | Описание |
|---|----------------|----------|
| 15 | `AddPermissionPolicies_RegistersProductPolicy` | Проверяет регистрацию политик `products.create`, `products.edit_any` и `products.delete` |
| 16 | `HandleRequirementAsync_WithCommaSeparatedPermissions_ShouldSucceed` | Успешно обрабатывает разрешения, переданные одной строкой через запятую |
| 17 | `HandleRequirementAsync_MissingPermission_ShouldFail` | Отклоняет запрос, если требуемого разрешения нет |

## ProductService.GetAll.Tests.cs (7 методов, 7 запусков)

| # | Название теста | Описание |
|---|----------------|----------|
| 18 | `CreateAsync_PersistsCategoryAndInactiveStatus` | Сохраняет категорию и статус активности при создании товара |
| 19 | `ProductService_Constructor_CreatesUploadDirectory` | Создаёт директорию `wwwroot/uploads` |
| 20 | `GetAllAsync_CategoryIdZero_ReturnsOnlyProductsInThatCategory` | Фильтрует товары без категории при `CategoryId = 0` |
| 21 | `GetAllAsync_Search_FiltersProductNamesWithoutCaseSensitivity` | Ищет по названию без учёта регистра |
| 22 | `GetAllAsync_SortPriceAscending_ReturnsProductsByPrice` | Сортирует товары по возрастанию цены |
| 23 | `GetAllAsync_Sqlite_SortPriceAscending_ReturnsProductsByPrice` | Проверяет сортировку по цене на SQLite |
| 24 | `GetAllAsync_Sqlite_SearchMatchesCyrillicWithoutCaseSensitivity` | Проверяет поиск кириллицы без учёта регистра на SQLite |

## ProductService.GetForEdit.Tests.cs (4 метода, 4 запуска)

| # | Название теста | Описание |
|---|----------------|----------|
| 25 | `GetForEditAsync_Admin_CanEditAnyProductEvenWithoutOwner` | Администратор может редактировать любой товар |
| 26 | `GetForEditAsync_Owner_CanEditOwnProduct` | Владелец может редактировать свой товар |
| 27 | `GetForEditAsync_NonOwnerSeller_CannotEdit_ReturnsNull` | Продавец, не являющийся владельцем, получает `null` |
| 28 | `GetForEditAsync_ProductNotFound_ReturnsNull` | Для несуществующего товара возвращается `null` |

## ProductService.GetManagedProducts.Tests.cs (4 метода, 4 запуска)

| # | Название теста | Описание |
|---|----------------|----------|
| 29 | `GetManagedProductsAsync_AdminUser_ReturnsAllProducts` | Администратор видит все товары |
| 30 | `GetManagedProductsAsync_SellerUser_ReturnsOnlyOwnedProducts` | Продавец видит только свои товары |
| 31 | `GetManagedProductsAsync_InvalidUserClaim_ReturnsNoProducts` | Пользователь без корректного идентификатора в claims не видит товары |
| 32 | `GetManagedProductsAsync_Pagination_CalculatesCorrectly` | Проверяет количество элементов, номер страницы и общее количество страниц |

## ProductService.Update.Tests.cs (4 метода, 4 запуска)

| # | Название теста | Описание |
|---|----------------|----------|
| 33 | `UpdateAsync_OwnerUpdatesText_KeepsExistingImages` | Обновление текста товара сохраняет существующие изображения |
| 34 | `UpdateAsync_OwnerAddsNewImage_AppendsItToExistingImages` | Новое изображение добавляется к существующим |
| 35 | `UpdateAsync_NonOwnerAttempt_ReturnsFalse` | Попытка обновления не владельцем возвращает `false` |
| 36 | `UpdateAsync_ProductNotFound_ReturnsFalse` | Обновление несуществующего товара возвращает `false` |

## ProductServiceTestFixture.cs

Общая тестовая среда для тестов `ProductService`. Для каждого теста создаёт отдельную In-Memory базу данных и уникальную временную папку `wwwroot/uploads`. Предоставляет тесту `AppDbContext` и настроенный `ProductService`; при освобождении fixture закрывает контекст и удаляет временную папку.

## Итого

- **36 тестовых методов**
- **48 отдельных запусков**, включая каждый набор данных в `[Theory]`
