using AsyncAPIDotNetCore.Contexts;
using AsyncAPIDotNetCore.Entities;
using AsyncAPIDotNetCore.ExternalModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AsyncAPIDotNetCore.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BookContext _bookContext;
        private readonly IHttpClientFactory _httpClient;

        public BooksRepository(BookContext bookContext, IHttpClientFactory httpClient)
        {
            _bookContext = bookContext ?? throw new ArgumentNullException(nameof(bookContext));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
                throw new ArgumentNullException(nameof(bookToAdd));

            _bookContext.Add(bookToAdd);
        }

        public async Task<Book> GetBookAsync(Guid id)
        {
            return await _bookContext.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
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

        public async Task<bool> SaveChangesAsync()
        {
            return (await _bookContext.SaveChangesAsync() > 0);
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            return await _bookContext.Books
                .Where(b => bookIds.Contains(b.Id))
                .Include(b => b.Author)
                .ToListAsync();
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            var httpClient = _httpClient.CreateClient();
            var response = await httpClient.GetAsync($"http://localhost:52644/api/bookCovers/{coverId}"); 

            if(response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<BookCover>(await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }

            return null;

        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClient.CreateClient();
            var bookCovers = new List<BookCover>();

            var bookCoverUrls = new[] {
                $"http://localhost:52644/api/bookCovers/{bookId}-1",
                $"http://localhost:52644/api/bookCovers/{bookId}-2",
                $"http://localhost:52644/api/bookCovers/{bookId}-3",
                $"http://localhost:52644/api/bookCovers/{bookId}-4",
                $"http://localhost:52644/api/bookCovers/{bookId}-5"
            };

            foreach (var cover in bookCoverUrls)
            {
                var response = await httpClient.GetAsync(cover);

                if (response.IsSuccessStatusCode)
                {
                    bookCovers.Add(JsonSerializer.Deserialize<BookCover>(await response.Content.ReadAsStringAsync(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                }
            }

            return bookCovers;
        }
    }
}
