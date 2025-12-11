using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Domain.Interfaces
{
    public interface IZipService
    {
        byte[] GenerateZipForDownload(IEnumerable<IItemValidating> items);
        Task ProcessUploadedZipAsync(Stream zipStream, List<IItemValidating> items);
    }
}

