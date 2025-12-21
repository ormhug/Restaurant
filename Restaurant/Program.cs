// Program.cs
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using DataAccess;
using Domain.Interfaces;
using Services;
using Microsoft.AspNetCore.Identity; // <--- ДОБАВЛЕНО: Для IdentityUser и IdentityRole

var builder = WebApplication.CreateBuilder(args);

// --- 1. Конфигурация Базы Данных ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// --- 1.5. Конфигурация Identity (ВХОД И РЕГИСТРАЦИЯ) ---
// ВАЖНО: Добавляем поддержку пользователей и ролей
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Упрощаем требования к паролю для тестов (чтобы не придумывать сложные)
    options.Password.RequireDigit = false;          // Не обязательно цифры
    options.Password.RequiredLength = 4;            // Минимум 4 символа
    options.Password.RequireNonAlphanumeric = false;// Не обязательно спецсимволы (!@#)
    options.Password.RequireUppercase = false;      // Не обязательно заглавные
    options.Password.RequireLowercase = false;      // Не обязательно строчные
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Храним пользователей в нашей БД
.AddDefaultTokenProviders();


// --- 2. Конфигурация Кэша ---
builder.Services.AddMemoryCache();


// --- 3. Регистрация Keyed Services (AA2.3.2) ---
const string InMemoryKey = "InMemory";
const string DbKey = "Database";

// 3.1. Регистрация ItemsInMemoryRepository
builder.Services.AddKeyedScoped<IItemsRepository, ItemsInMemoryRepository>(InMemoryKey);

// 3.2. Регистрация ItemsDbRepository
builder.Services.AddKeyedScoped<IItemsRepository, ItemsDbRepository>(DbKey);


// --- 4. Регистрация Сервисов и MVC ---
builder.Services.AddScoped<IZipService, ZipService>();
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- ВАЖНО: ПОРЯДОК ИМЕЕТ ЗНАЧЕНИЕ ---
// Сначала проверяем КТО ЭТО (Authentication),
// потом проверяем ЧТО ЕМУ МОЖНО (Authorization).
app.UseAuthentication(); // <--- ДОБАВЛЕНО
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Items}/{action=Catalog}/{id?}");

app.Run();