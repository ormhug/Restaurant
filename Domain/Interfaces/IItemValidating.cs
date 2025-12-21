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

        string Status { get; set; }

        // Возвращает список email-адресов, которые могут одобрить элемент
        List<string> GetValidators();

        // Возвращает имя Partial View для отображения карточки
        string GetCardPartial();

        List<string> Validate();

        string ImagePath { get; set; }

        string UniqueImportId { get; }

        string GetId();
    }
}
