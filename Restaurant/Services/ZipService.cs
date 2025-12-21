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

        public ZipService(IWebHostEnvironment env)
        {
            _env = env;
        }



        public byte[] GenerateZipForDownload(IEnumerable<IItemValidating> items)
        {
            // заглушка для default.jpg: текстовый плейсхолдер
            var defaultImageContent = Encoding.UTF8.GetBytes("Image placeholder.");

            using (var memoryStream = new MemoryStream())
            {
                // ZipArchiveMode.Create создает новый архив
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in items)
                    {
                        //  для именования папки
                        string itemId = item.UniqueImportId;

                        // Имя файла в zip item-<UniqueImportId>/default.jpg
                        string entryName = $"item-{itemId}/default.jpg";

                        var entry = archive.CreateEntry(entryName);

                        using (var entryStream = entry.Open())
                        {
                            //записываем содержимое заглушки
                            entryStream.Write(defaultImageContent, 0, defaultImageContent.Length);
                        }
                    }
                }
                
                return memoryStream.ToArray();
            }
        }


        public async Task ProcessUploadedZipAsync(Stream zipStream, List<IItemValidating> items)
        {
            // словарь для быстрого поиска элемента по его ID (item-<id>)
            var itemMap = items.ToDictionary(
                item => $"item-{item.UniqueImportId}",
                item => item);

            // путь для сохранения изображений в wwwroot/images
            string imagesPath = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(imagesPath); // Убедимся что папка существует

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    // ищет только файлы, которые выглядят как изображения и находятся в папке item-<id>
                    if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && entry.Length > 0)
                    {
                        // FullName может быть "item-R1/image.jpg"

                        // извлекаем имя папки (т.е. ключ)
                        string folderName = Path.GetDirectoryName(entry.FullName)?.Replace('\\', '/');

                        if (folderName != null && itemMap.ContainsKey(folderName))
                        {
                            var itemToUpdate = itemMap[folderName];

                            //  уникальное имя файла для сохранения 
                            string extension = Path.GetExtension(entry.Name);
                            string uniqueFileName = Guid.NewGuid().ToString() + extension;
                            string savePath = Path.Combine(imagesPath, uniqueFileName);

                            //сохранение файла на диск асинхронно
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(savePath, FileMode.Create))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }

                            // обновление ссылкы в сущности (относительный путь для UI)
                            itemToUpdate.ImagePath = $"/images/{uniqueFileName}";
                        }
                    }
                }
            }





        }

    }

}

