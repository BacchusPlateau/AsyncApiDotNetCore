using AsyncAPIDotNetCore.Contexts;
using AsyncAPIDotNetCore.Entities;
using AsyncAPIDotNetCore.ExternalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAPIDotNetCore.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BookContext _bookContext;
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public BooksRepository(BookContext bookContext, IHttpClientFactory httpClient, ILogger<BooksRepository> logger)
        {
            _bookContext = bookContext ?? throw new ArgumentNullException(nameof(bookContext));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            //uncomment to test
            //var pageCalculator = new Books.Legacy.ComplicatedPageCalculator();
            //var amountOfPages = pageCalculator.CalculateBookPages();

            //await _bookContext.Database.ExecuteSqlRawAsync("WAITFOR DELAY '00:00:02';");  //simulate network traffic

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

                if(_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
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
            //_bookContext.Database.ExecuteSqlRaw("WAITFOR DELAY '00:00:02';"); //simulate network traffic

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

        private async Task<BookCover> DownloadBookCoverAsync(HttpClient httpClient, string bookCoverUrl, CancellationToken cancellationToken)
        {
            var response = await httpClient.GetAsync(bookCoverUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonSerializer.Deserialize<BookCover>(await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                return bookCover;
            }

            _cancellationTokenSource.Cancel();
            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClient.CreateClient();
            var bookCovers = new List<BookCover>();
            _cancellationTokenSource = new CancellationTokenSource();

            var bookCoverUrls = new[] {
                $"http://localhost:52644/api/bookCovers/{bookId}-1",
                $"http://localhost:52644/api/bookCovers/{bookId}-2",
                $"http://localhost:52644/api/bookCovers/{bookId}-3", //?returnFault=true",   UNCOMMENT TO TEST CANCEL TOKENs
                $"http://localhost:52644/api/bookCovers/{bookId}-4",
                $"http://localhost:52644/api/bookCovers/{bookId}-5"
            };

            // use LINQ to create tasks, taking advantage of LINQ's intrinsic deferred execution quality
            var downloadBookCoverTasksQuery =
                from bookCoverUrl
                in bookCoverUrls
                select DownloadBookCoverAsync(httpClient, bookCoverUrl, _cancellationTokenSource.Token);

            //start the tasks
            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();

            //we will wait for tasks to complete
            //writing the code this way allows for immediate post-processing of any task you may want to do after the download is completed
            //the other method commented otu below will execute in order (1..n) depending on how many calls we have to the httpClient

            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }catch (OperationCanceledException operationCanceledException)
            {
                _logger.LogInformation($"{operationCanceledException.Message}");
                foreach(var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return new List<BookCover>();
            } catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                throw;
            }

            //foreach (var cover in bookCoverUrls)
            //{
            //    var response = await httpClient.GetAsync(cover);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        bookCovers.Add(JsonSerializer.Deserialize<BookCover>(await response.Content.ReadAsStringAsync(),
            //            new JsonSerializerOptions
            //            {
            //                PropertyNameCaseInsensitive = true
            //            }));
            //    }
            //}

            //return bookCovers;
        }
    }
}
