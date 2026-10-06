using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Mish.Api.Data;
using Mish.Api.Models;

const int batchSize = 500;
const string efProductVersion = "10.0.11";
var checkOnly = args.Length == 1 && args[0] == "--check";
if (args.Length > (checkOnly ? 1 : 0) || (args.Length == 1 && !checkOnly))
{
    throw new ArgumentException("Usage: Mish.DataMigrator [--check]");
}

var sqlitePath = Environment.GetEnvironmentVariable("MISH_SQLITE_PATH");
sqlitePath = string.IsNullOrWhiteSpace(sqlitePath)
    ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Mish.Api/app.db"))
    : Path.GetFullPath(sqlitePath);

if (!File.Exists(sqlitePath))
{
    throw new FileNotFoundException(
        $"SQLite database was not found at '{sqlitePath}'. Set MISH_SQLITE_PATH to its location.",
        sqlitePath);
}

var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite($"Data Source={sqlitePath}")
    .Options;

await using var source = new AppDbContext(sqliteOptions);
if (!await source.Database.CanConnectAsync())
{
    throw new InvalidOperationException($"Could not connect to SQLite database at '{sqlitePath}'.");
}

var sourceCounts = await GetCountsAsync(source);
Console.WriteLine($"SQLite source: {sqlitePath}");
PrintCounts(sourceCounts);
ValidatePostgresModel();

if (checkOnly)
{
    await ValidateSourceAsync(source);
    Console.WriteLine("Check complete. No database was changed.");
    return;
}

var postgresConnectionString =
    Environment.GetEnvironmentVariable("MISH_POSTGRES_CONNECTION")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(postgresConnectionString)
    || postgresConnectionString.Contains("******", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Set MISH_POSTGRES_CONNECTION to a real PostgreSQL connection string. Do not put credentials in source code.");
}

var postgresOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(postgresConnectionString)
    .Options;

await using var target = new AppDbContext(postgresOptions);
try
{
    await target.Database.OpenConnectionAsync();
}
catch (DbException ex)
{
    throw new InvalidOperationException(
        $"Could not connect to PostgreSQL: {ex.Message} Check that the server is running, " +
        "the host/port/database/user are correct, and the password is valid.",
        ex);
}

Console.WriteLine("Preparing PostgreSQL schema...");
await PreparePostgresSchemaAsync(target);

await using var transaction = await target.Database.BeginTransactionAsync();
await EnsureMigrationHistoryAsync(target);

var copied = new Dictionary<string, long>
{
    ["Users"] = await TransferAsync(
        await source.Users.AsNoTracking().ToListAsync(), target.Users, target),
    ["Roles"] = await TransferAsync(
        await source.Roles.AsNoTracking().ToListAsync(), target.Roles, target),
    ["Permissions"] = await TransferAsync(
        await source.Permissions.AsNoTracking().ToListAsync(), target.Permissions, target),
};

var categories = await source.Categories.AsNoTracking().ToListAsync();
copied["Categories"] = await TransferAsync(
    SortCategoriesByParent(categories), target.Categories, target);
copied["Products"] = await TransferAsync(
    await source.Products.AsNoTracking().ToListAsync(), target.Products, target);
copied["RolePermissions"] = await TransferAsync(
    await source.RolePermissions.AsNoTracking().ToListAsync(), target.RolePermissions, target);
copied["ProductSellers"] = await TransferAsync(
    await source.Set<ProductSeller>().AsNoTracking().ToListAsync(),
    target.Set<ProductSeller>(), target);
copied["CartItems"] = await TransferAsync(
    await source.CartItems.AsNoTracking().ToListAsync(), target.CartItems, target);
copied["Orders"] = await TransferAsync(
    await source.Orders.AsNoTracking().ToListAsync(), target.Orders, target);
copied["OrderItems"] = await TransferAsync(
    await source.OrderItems.AsNoTracking().ToListAsync(), target.OrderItems, target);

foreach (var (table, sourceCount) in sourceCounts)
{
    var destinationCount = await GetCountAsync(target, table);
    if (destinationCount != sourceCount)
    {
        throw new InvalidOperationException(
            $"Verification failed for {table}: SQLite has {sourceCount} rows, PostgreSQL has {destinationCount} rows.");
    }
}

await SynchronizeSequencesAsync(target);
await transaction.CommitAsync();

Console.WriteLine("Transfer completed and verified:");
PrintCounts(copied);

static async Task<Dictionary<string, long>> GetCountsAsync(AppDbContext context)
{
    return new Dictionary<string, long>
    {
        ["Users"] = await context.Users.LongCountAsync(),
        ["Roles"] = await context.Roles.LongCountAsync(),
        ["Permissions"] = await context.Permissions.LongCountAsync(),
        ["Categories"] = await context.Categories.LongCountAsync(),
        ["Products"] = await context.Products.LongCountAsync(),
        ["RolePermissions"] = await context.RolePermissions.LongCountAsync(),
        ["ProductSellers"] = await context.Set<ProductSeller>().LongCountAsync(),
        ["CartItems"] = await context.CartItems.LongCountAsync(),
        ["Orders"] = await context.Orders.LongCountAsync(),
        ["OrderItems"] = await context.OrderItems.LongCountAsync()
    };
}

static void ValidatePostgresModel()
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql("Host=localhost;Database=validation_only;Username=validation_only;Password=validation_only")
        .Options;
    using var context = new AppDbContext(options);
    var createScript = context.Database.GenerateCreateScript();

    foreach (var table in context.Model.GetEntityTypes()
                 .Select(entityType => entityType.GetTableName())
                 .Where(tableName => tableName is not null))
    {
        if (!createScript.Contains($"CREATE TABLE \"{table}\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"PostgreSQL schema generation did not include the expected table '{table}'.");
        }
    }
}

static async Task ValidateSourceAsync(AppDbContext source)
{
    var users = await source.Users.AsNoTracking().ToListAsync();
    var roles = await source.Roles.AsNoTracking().ToListAsync();
    var permissions = await source.Permissions.AsNoTracking().ToListAsync();
    var categories = await source.Categories.AsNoTracking().ToListAsync();
    var products = await source.Products.AsNoTracking().ToListAsync();
    var rolePermissions = await source.RolePermissions.AsNoTracking().ToListAsync();
    var productSellers = await source.Set<ProductSeller>().AsNoTracking().ToListAsync();
    var cartItems = await source.CartItems.AsNoTracking().ToListAsync();
    var orders = await source.Orders.AsNoTracking().ToListAsync();
    var orderItems = await source.OrderItems.AsNoTracking().ToListAsync();

    _ = SortCategoriesByParent(categories);

    var userIds = users.Select(user => user.Id).ToHashSet();
    var roleIds = roles.Select(role => role.Id).ToHashSet();
    var permissionIds = permissions.Select(permission => permission.Id).ToHashSet();
    var productIds = products.Select(product => product.Id).ToHashSet();
    var orderIds = orders.Select(order => order.Id).ToHashSet();

    RequireReferences(
        rolePermissions.Any(item =>
            !roleIds.Contains(item.RoleId) || !permissionIds.Contains(item.PermissionId)),
        "RolePermissions contains a missing role or permission.");
    RequireReferences(
        productSellers.Any(item =>
            !productIds.Contains(item.ProductId) || !userIds.Contains(item.SellerId)),
        "ProductSellers contains a missing product or seller.");
    RequireReferences(
        cartItems.Any(item =>
            !userIds.Contains(item.UserId) || !productIds.Contains(item.ProductId)),
        "CartItems contains a missing user or product.");
    RequireReferences(
        orders.Any(order => !userIds.Contains(order.UserId)),
        "Orders contains a missing user.");
    RequireReferences(
        orderItems.Any(item => !orderIds.Contains(item.OrderId)),
        "OrderItems contains a missing order.");
}

static void RequireReferences(bool hasMissingReference, string error)
{
    if (hasMissingReference)
    {
        throw new InvalidOperationException(error);
    }
}

static async Task<long> GetCountAsync(AppDbContext context, string table)
{
    var entityType = context.Model.GetEntityTypes()
        .Single(entity => entity.GetTableName() == table);
    var tableName = context.GetService<ISqlGenerationHelper>()
        .DelimitIdentifier(table, entityType.GetSchema());

    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
    if (context.Database.CurrentTransaction is { } currentTransaction)
    {
        command.Transaction = currentTransaction.GetDbTransaction();
    }

    if (command.Connection!.State != System.Data.ConnectionState.Open)
    {
        await command.Connection.OpenAsync();
    }

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt64(result);
}

static void PrintCounts(IReadOnlyDictionary<string, long> counts)
{
    foreach (var (table, count) in counts)
    {
        Console.WriteLine($"  {table}: {count}");
    }
}

static async Task PreparePostgresSchemaAsync(AppDbContext target)
{
    var modelTables = target.Model.GetEntityTypes()
        .Select(entityType => entityType.GetTableName())
        .Where(tableName => tableName is not null)
        .Cast<string>()
        .ToHashSet(StringComparer.Ordinal);

    var existingTables = await ReadStringsAsync(
        target,
        "SELECT table_name FROM information_schema.tables " +
        "WHERE table_schema = 'public' AND table_type = 'BASE TABLE'");

    var applicationTables = existingTables.Intersect(modelTables, StringComparer.Ordinal).ToHashSet();
    var metadataTables = new HashSet<string>(StringComparer.Ordinal)
    {
        "__EFMigrationsHistory",
        "__EFMigrationsLock"
    };
    var unrelatedTables = existingTables
        .Except(modelTables, StringComparer.Ordinal)
        .Except(metadataTables, StringComparer.Ordinal)
        .ToArray();

    if (unrelatedTables.Length > 0)
    {
        throw new InvalidOperationException(
            $"PostgreSQL database contains unrelated tables ({string.Join(", ", unrelatedTables)}). " +
            "Use a separate, empty database for the transfer.");
    }

    if (applicationTables.Count == 0)
    {
        foreach (var metadataTable in existingTables.Intersect(metadataTables, StringComparer.Ordinal))
        {
            var dropStatement = metadataTable switch
            {
                "__EFMigrationsHistory" => "DROP TABLE \"__EFMigrationsHistory\"",
                "__EFMigrationsLock" => "DROP TABLE \"__EFMigrationsLock\"",
                _ => throw new InvalidOperationException("Unexpected EF metadata table.")
            };
            await target.Database.ExecuteSqlRawAsync(dropStatement);
        }

        if (!await target.Database.EnsureCreatedAsync())
        {
            throw new InvalidOperationException(
                "PostgreSQL schema could not be initialized. Use a new, empty database and run the migrator again.");
        }
    }
    else
    {
        var missingTables = modelTables.Except(applicationTables, StringComparer.Ordinal).ToArray();
        if (missingTables.Length > 0)
        {
            throw new InvalidOperationException(
                $"PostgreSQL schema is incomplete (missing tables: {string.Join(", ", missingTables)}). " +
                "Use a new, empty database; the migrator will not delete or repair existing tables.");
        }

        await ValidateColumnsAsync(target);

        foreach (var table in modelTables)
        {
            if (await HasRowsAsync(target, table))
            {
                throw new InvalidOperationException(
                    $"PostgreSQL table '{table}' already contains data. Transfer stopped to prevent duplicates. " +
                    "Use a separate, empty database.");
            }
        }
    }

    await ValidateColumnsAsync(target);

    await target.Database.ExecuteSqlRawAsync(
        """
        CREATE OR REPLACE FUNCTION public.unicode_lower(value text)
        RETURNS text
        LANGUAGE sql
        IMMUTABLE
        PARALLEL SAFE
        AS $$ SELECT lower(value) $$;
        """);
}

static async Task ValidateColumnsAsync(AppDbContext target)
{
    var actualColumns = await ReadStringsAsync(
        target,
        "SELECT table_name || '.' || column_name FROM information_schema.columns " +
        "WHERE table_schema = 'public'");
    var actual = actualColumns.ToHashSet(StringComparer.Ordinal);
    var missing = new List<string>();

    foreach (var entityType in target.Model.GetEntityTypes())
    {
        var tableName = entityType.GetTableName();
        if (tableName is null)
        {
            continue;
        }

        var storeObject = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
        foreach (var property in entityType.GetProperties())
        {
            var columnName = property.GetColumnName(storeObject);
            if (columnName is not null && !actual.Contains($"{tableName}.{columnName}"))
            {
                missing.Add($"{tableName}.{columnName}");
            }
        }
    }

    if (missing.Count > 0)
    {
        throw new InvalidOperationException(
            $"PostgreSQL schema is missing columns ({string.Join(", ", missing)}). " +
            "Use a new, empty database; the migrator will not alter existing tables.");
    }
}

static async Task<bool> HasRowsAsync(AppDbContext target, string table)
{
    var entityType = target.Model.GetEntityTypes()
        .Single(entity => entity.GetTableName() == table);
    var sqlHelper = target.GetService<ISqlGenerationHelper>();
    var tableName = sqlHelper.DelimitIdentifier(table, entityType.GetSchema());

    await using var command = target.Database.GetDbConnection().CreateCommand();
    command.CommandText = $"SELECT EXISTS (SELECT 1 FROM {tableName} LIMIT 1)";
    if (command.Connection!.State != System.Data.ConnectionState.Open)
    {
        await command.Connection.OpenAsync();
    }

    return Convert.ToBoolean(await command.ExecuteScalarAsync());
}

static async Task<List<string>> ReadStringsAsync(AppDbContext context, string sql)
{
    var values = new List<string>();
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = sql;

    if (command.Connection!.State != System.Data.ConnectionState.Open)
    {
        await command.Connection.OpenAsync();
    }

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        values.Add(reader.GetString(0));
    }

    return values;
}

static async Task EnsureMigrationHistoryAsync(AppDbContext target)
{
    await target.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        )
        """);

    var migrationIds = target.GetService<IMigrationsAssembly>().Migrations.Keys;
    foreach (var migrationId in migrationIds)
    {
        await target.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({migrationId}, {efProductVersion}) ON CONFLICT (\"MigrationId\") DO NOTHING");
    }
}

static async Task<long> TransferAsync<TEntity>(
    IEnumerable<TEntity> entities,
    DbSet<TEntity> destination,
    AppDbContext target)
    where TEntity : class
{
    var dateTimeProperties = typeof(TEntity)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property =>
            property.PropertyType == typeof(DateTime)
            || property.PropertyType == typeof(DateTime?))
        .ToArray();

    long copied = 0;
    foreach (var entity in entities)
    {
        NormalizeDateTimes(entity, dateTimeProperties);
        destination.Add(entity);
        copied++;

        if (copied % batchSize == 0)
        {
            await target.SaveChangesAsync();
            target.ChangeTracker.Clear();
        }
    }

    if (copied % batchSize != 0)
    {
        await target.SaveChangesAsync();
    }

    target.ChangeTracker.Clear();
    return copied;
}

static void NormalizeDateTimes<TEntity>(TEntity entity, PropertyInfo[] properties)
    where TEntity : class
{
    foreach (var property in properties)
    {
        if (property.GetValue(entity) is not DateTime dateTime)
        {
            continue;
        }

        var utcDateTime = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
        property.SetValue(entity, utcDateTime);
    }
}

static IReadOnlyList<Category> SortCategoriesByParent(IReadOnlyCollection<Category> categories)
{
    var categoriesById = categories.ToDictionary(category => category.Id);
    var sorted = new List<Category>(categories.Count);
    var visited = new HashSet<int>();
    var visiting = new HashSet<int>();

    void Visit(Category category)
    {
        if (visited.Contains(category.Id))
        {
            return;
        }

        if (!visiting.Add(category.Id))
        {
            throw new InvalidOperationException(
                $"Category hierarchy contains a cycle involving category {category.Id}.");
        }

        if (category.ParentId is int parentId)
        {
            if (!categoriesById.TryGetValue(parentId, out var parent))
            {
                throw new InvalidOperationException(
                    $"Category {category.Id} references missing parent category {parentId}.");
            }

            Visit(parent);
        }

        visiting.Remove(category.Id);
        visited.Add(category.Id);
        sorted.Add(category);
    }

    foreach (var category in categories)
    {
        Visit(category);
    }

    return sorted;
}

static async Task SynchronizeSequencesAsync(AppDbContext target)
{
    var sqlHelper = target.GetService<ISqlGenerationHelper>();

    foreach (var entityType in target.Model.GetEntityTypes())
    {
        var tableName = entityType.GetTableName();
        var primaryKey = entityType.FindPrimaryKey();
        if (tableName is null
            || primaryKey is null
            || primaryKey.Properties.Count != 1
            || primaryKey.Properties[0].ValueGenerated != ValueGenerated.OnAdd
            || primaryKey.Properties[0].ClrType != typeof(int))
        {
            continue;
        }

        var storeObject = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
        var columnName = primaryKey.Properties[0].GetColumnName(storeObject);
        if (columnName is null)
        {
            continue;
        }

        var delimitedTable = sqlHelper.DelimitIdentifier(tableName, entityType.GetSchema());
        var delimitedColumn = sqlHelper.DelimitIdentifier(columnName);
        var tableLiteral = delimitedTable.Replace("'", "''", StringComparison.Ordinal);
        var columnLiteral = columnName.Replace("'", "''", StringComparison.Ordinal);
        var sql =
            $"SELECT setval(pg_get_serial_sequence('{tableLiteral}', '{columnLiteral}'), " +
            $"COALESCE(MAX({delimitedColumn}), 1), MAX({delimitedColumn}) IS NOT NULL) FROM {delimitedTable}";

        await target.Database.ExecuteSqlRawAsync(sql);
    }
}
