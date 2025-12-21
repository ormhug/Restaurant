using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using DataAccess;
using Domain.Interfaces;
using Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

//конфигурация бд
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// конфигурация аутентификации и авторизации
// поддержку пользователей и ролей
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // простые пароли для упрощения
    options.Password.RequireDigit = false; // Не обязательно цифры
    options.Password.RequiredLength = 4;            
    options.Password.RequireNonAlphanumeric = false; // Не обязательно спецсимволы (!@#)
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>() //юзеры в бд
.AddDefaultTokenProviders();


// конфигурация кеша
builder.Services.AddMemoryCache();


//регистрация keyed services
const string InMemoryKey = "InMemory";
const string DbKey = "Database";


builder.Services.AddKeyedScoped<IItemsRepository, ItemsInMemoryRepository>(InMemoryKey);

builder.Services.AddKeyedScoped<IItemsRepository, ItemsDbRepository>(DbKey);


// регистрация mvc и сервисов
builder.Services.AddScoped<IZipService, ZipService>();
builder.Services.AddControllersWithViews();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ПОРЯДОК ИМЕЕТ СМЫСЛ
// Сначала проверяем КТО ЭТО (Authentication),
// потом проверяем ЧТО ЕМУ МОЖНО (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Items}/{action=Catalog}/{id?}");

app.Run();