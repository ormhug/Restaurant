using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IItemValidating
    {
        // Должен быть реализован в Restaurant и MenuItem
        string Status { get; set; }

        // Возвращает список email-адресов, которые могут одобрить элемент [cite: 52]
        List<string> GetValidators();

        // Возвращает имя Partial View для отображения карточки [cite: 54]
        string GetCardPartial();
    }
}