using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using DataAccess;
using Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Конфигурация Базы Данных ---
// (Предполагается, что строка подключения 'DefaultConnection' настроена в appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        // Указываем, где находится ApplicationDbContext для миграций
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// --- 2. Конфигурация Кэша ---
// Регистрация IMemoryCache (требуется по критериям)
builder.Services.AddMemoryCache();


// --- 3. Регистрация Keyed Services (AA2.3.2) ---

// Определяем ключи для использования в контроллерах:
const string InMemoryKey = "InMemory";
const string DbKey = "Database";

// 3.1. Регистрация ItemsInMemoryRepository
// AddKeyedScoped означает: использовать ItemsInMemoryRepository, когда запрошен IItemsRepository с ключом "InMemory".
builder.Services.AddKeyedScoped<IItemsRepository, ItemsInMemoryRepository>(InMemoryKey);

// 3.2. Регистрация ItemsDbRepository
builder.Services.AddKeyedScoped<IItemsRepository, ItemsDbRepository>(DbKey);


// Добавление других сервисов (MVC/Razor Pages)
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
