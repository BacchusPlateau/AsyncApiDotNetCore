using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncAPIDotNetCore.Services
{
    public interface IBooksRepository
    {
        IEnumerable<Entities.Book> GetBooks();
        Task<IEnumerable<Entities.Book>> GetBooksAync();
        Task<Entities.Book> GetBookAsync(Guid id);
    }
}
