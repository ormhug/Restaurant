using Domain.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace Services
{

    public class ZipService : IZipService
    {

        private readonly IWebHostEnvironment _env;

        // ОБНОВЛЕНИЕ: Конструктор для инъекции IWebHostEnvironment
        public ZipService(IWebHostEnvironment env)
        {
            _env = env;
        }



        // Метод генерации ZIP-файла с заглушками для скачивания (AA4.3.3)
        public byte[] GenerateZipForDownload(IEnumerable<IItemValidating> items)
        {
            // Создаем заглушку для default.jpg: текстовый плейсхолдер
            var defaultImageContent = Encoding.UTF8.GetBytes("Image placeholder.");

            using (var memoryStream = new MemoryStream())
            {
                // ZipArchiveMode.Create создает новый архив
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in items)
                    {
                        // ИСПОЛЬЗУЕМ UniqueImportId для именования папки (AA4.3.3)
                        string itemId = item.UniqueImportId;

                        // Имя файла в ZIP: item-<UniqueImportId>/default.jpg
                        string entryName = $"item-{itemId}/default.jpg";

                        var entry = archive.CreateEntry(entryName);

                        using (var entryStream = entry.Open())
                        {
                            // Записываем содержимое заглушки
                            entryStream.Write(defaultImageContent, 0, defaultImageContent.Length);
                        }
                    }
                }
                // Возвращаем байты ZIP-архива
                return memoryStream.ToArray();
            }
        }

        // Этот метод мы будем реализовывать на следующем шаге (AA4.3.4)
        public async Task ProcessUploadedZipAsync(Stream zipStream, List<IItemValidating> items)
        {
            // Создаем словарь для быстрого поиска элемента по его ID (item-<id>)
            // Ключ: item-<id> (например, item-R123 или item-M1a2b3c)
            var itemMap = items.ToDictionary(
                item => $"item-{item.UniqueImportId}",
                item => item);

            // Определяем путь для сохранения изображений в wwwroot/images
            string imagesPath = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(imagesPath); // Убедимся, что папка существует

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    // Ищем только файлы, которые выглядят как изображения и находятся в папке item-<id>
                    if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && entry.Length > 0)
                    {
                        // FullName может быть "item-R1/image.jpg"

                        // 1. Извлекаем имя папки (т.е. ключ)
                        string folderName = Path.GetDirectoryName(entry.FullName)?.Replace('\\', '/');

                        if (folderName != null && itemMap.ContainsKey(folderName))
                        {
                            var itemToUpdate = itemMap[folderName];

                            // 2. Генерируем уникальное имя файла для сохранения (AA4.3.4)
                            string extension = Path.GetExtension(entry.Name);
                            string uniqueFileName = Guid.NewGuid().ToString() + extension;
                            string savePath = Path.Combine(imagesPath, uniqueFileName);

                            // 3. Сохраняем файл на диск асинхронно
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(savePath, FileMode.Create))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }

                            // 4. Обновляем ссылку в сущности (относительный путь для UI)
                            itemToUpdate.ImagePath = $"/images/{uniqueFileName}";
                        }
                    }
                }
            }





        }

    }

}

