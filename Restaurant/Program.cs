// Program.cs
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using DataAccess;
using Domain.Interfaces;
using Services; // Используем правильное пространство имен для ZipService

var builder = WebApplication.CreateBuilder(args);

// --- 1. Конфигурация Базы Данных ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        // Указываем, где находится ApplicationDbContext для миграций
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// --- 2. Конфигурация Кэша ---
builder.Services.AddMemoryCache();


// --- 3. Регистрация Keyed Services (AA2.3.2) ---

// Определяем ключи для использования в контроллерах:
const string InMemoryKey = "InMemory";
const string DbKey = "Database";

// 3.1. Регистрация ItemsInMemoryRepository
builder.Services.AddKeyedScoped<IItemsRepository, ItemsInMemoryRepository>(InMemoryKey);

// 3.2. Регистрация ItemsDbRepository
builder.Services.AddKeyedScoped<IItemsRepository, ItemsDbRepository>(DbKey);


// --- 4. Регистрация Сервисов и MVC ---

// Регистрация ZipService (он инжектирует IWebHostEnvironment, который зарегистрирован автоматически)
builder.Services.AddScoped<IZipService, ZipService>();

// Добавление других сервисов (MVC/Razor Pages). Удалили дубликат!
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ВАЖНО: Разрешает доступ к wwwroot для изображений

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();