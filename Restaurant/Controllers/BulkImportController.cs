using System.Linq;
using System.Threading.Tasks;
using Domain; // Добавили using для ImportItemFactory
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Добавили using для IFormFile

// Ключи, которые мы определили в Program.cs:
// const string InMemoryKey = "InMemory"; 
// const string DbKey = "Database";      


public class BulkImportController : Controller
{
    const string InMemoryKey = "InMemory";
    const string DbKey = "Database";

    // AA2.3.4: Метод BulkImport использует только InMemory репозиторий
    [HttpPost]
    public async Task<IActionResult> BulkImport( // Сделали метод асинхронным
        IFormFile jsonFile,
        // Инъекция ItemsInMemoryRepository по ключу "InMemory"
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            // Здесь может быть логирование или возврат ошибки пользователю
            return BadRequest("File not provided.");
        }

        // Используем Stream для чтения файла
        using (var stream = jsonFile.OpenReadStream())
        {
            // 1. Вызов ImportItemFactory.CreateAsync (AA2.3.1)
            var parsedItems = await ImportItemFactory.CreateAsync(stream);

            // 2. Вызов await inMemoryRepo.SaveAsync(parsedItems)
            await inMemoryRepo.SaveAsync(parsedItems);
        }

        // Перенаправляем на страницу предпросмотра
        return View("Preview");
    }

    // AA4.3: Метод Commit использует оба репозитория
    [HttpPost]
    public async Task<IActionResult> Commit( // Сделали метод асинхронным
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo,
        [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
    {
        // 1. Чтение данных из кэша: var items = await inMemoryRepo.GetAsync();
        var itemsToCommit = await inMemoryRepo.GetAsync();

        if (!itemsToCommit.Any())
        {
            return RedirectToAction("Catalog", "Items");
        }

        // 2. Сохранение в БД: await dbRepo.SaveAsync(items);
        await dbRepo.SaveAsync(itemsToCommit);

        // 3. Очистка кэша: inMemoryRepo.Clear();
        // Clear обычно синхронен, но если он асинхронен, нужно добавить await.
        // Предполагаем, что Clear в ItemsInMemoryRepository синхронен.
        inMemoryRepo.Clear();

        return RedirectToAction("Catalog", "Items");
    }
}