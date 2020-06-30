using AsyncAPIDotNetCore.Contexts;
using AsyncAPIDotNetCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncAPIDotNetCore.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BookContext _bookContext;

        public BooksRepository(BookContext bookContext)
        {
            _bookContext = bookContext ?? throw new ArgumentNullException(nameof(bookContext));
        }


        public async Task<Book> GetBookAsync(Guid id)
        {
            return await _bookContext.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> GetBooksAync()
        {
            await _bookContext.Database.ExecuteSqlRawAsync("WAITFOR DELAY '00:00:02';");  //simulate network traffic
            return await _bookContext.Books
                .Include(b => b.Author)
                .ToListAsync();
        }


        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_bookContext != null)
                {
                    _bookContext.Dispose();
                    _bookContext = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<Book> GetBooks()
        {
            _bookContext.Database.ExecuteSqlRaw("WAITFOR DELAY '00:00:02';"); //simulate network traffic
            return _bookContext.Books
                .Include(b => b.Author)
                .ToList();
        }
    }
}
