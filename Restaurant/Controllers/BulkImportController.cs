using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;

public class BulkImportController : Controller
{
    const string InMemoryKey = "InMemory";
    const string DbKey = "Database";
    private readonly IZipService _zipService; // Поле для инжектированного сервиса

    // Конструктор для Dependency Injection
    public BulkImportController(IZipService zipService)
    {
        _zipService = zipService;
    }


    [HttpPost]
    public async Task<IActionResult> BulkImport(
        IFormFile jsonFile,
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            return BadRequest("File not provided.");
        }

        using (var stream = jsonFile.OpenReadStream())
        {
            var parsedItems = await ImportItemFactory.CreateAsync(stream);

            var validItems = new List<IItemValidating>();
            var errorsDictionary = new Dictionary<IItemValidating, List<string>>();

            foreach (var item in parsedItems)
            {
                var itemErrors = item.Validate();

                if (itemErrors.Any())
                {
                    // Добавление ошибок...
                    errorsDictionary.Add(item, itemErrors);
                }
                else
                {
                    validItems.Add(item);
                }
            }

            // Сохранение ТОЛЬКО валидных элементов в кэш
            await inMemoryRepo.SaveAsync(validItems);

            // Генерация zip файла 
            var zipBytes = _zipService.GenerateZipForDownload(validItems);

            // Возвращаем файл для скачивания
            return File(
                zipBytes,
                "application/zip",
                "Items_for_Image_Upload.zip");
        }

 
    }

    //новый гет метод для отображения страницы предпросмотра после скачивания ZIP
    [HttpGet]
    public IActionResult Preview()
    {
        return View();
    }

    [HttpGet] 
    public IActionResult Index()
    {
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Commit(
        IFormFile zipFile, //Принимает zip файл с изображениями
        [FromKeyedServices(InMemoryKey)] IItemsRepository inMemoryRepo,
        [FromKeyedServices(DbKey)] IItemsRepository dbRepo)
    {
        // чтение данных из кэша
        var itemsToCommit = (await inMemoryRepo.GetAsync()).ToList();

        if (!itemsToCommit.Any())
        {
            return RedirectToAction("Catalog", "Items");
        }

        // Обработка и сохранение изображений из zip
        if (zipFile != null && zipFile.Length > 0)
        {
            using (var stream = zipFile.OpenReadStream())
            {
                await _zipService.ProcessUploadedZipAsync(stream, itemsToCommit);
            }
        }

        // сохранение в бд
        await dbRepo.SaveAsync(itemsToCommit);

        //Очистка кэша:
        inMemoryRepo.Clear();

        TempData["SuccessMessage"] = "Импорт данных и изображений завершен!";
        return RedirectToAction("Catalog", "Items");
    }
}