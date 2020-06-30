using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAPIDotNetCore.Filters;
using AsyncAPIDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAPIDotNetCore.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;

        public BooksController(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
        }

        [HttpGet]
        [BooksResultFilter]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksRepository.GetBooksAync();
            return Ok(books);
        }

        [HttpGet]
        [Route("{id}", Name = "GetBook")]
        [BookResultFilter]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);
            if (book == null)
                return NotFound();

            return Ok(book);
        }
    }
}
