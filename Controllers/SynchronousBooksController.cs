using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAPIDotNetCore.Filters;
using AsyncAPIDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAPIDotNetCore.Controllers
{
    [Route("api/synchronousbooks")]
    [ApiController]
    public class SynchronousBooksController : ControllerBase
    {
        private readonly IBooksRepository _books;

        public SynchronousBooksController(IBooksRepository books)
        {
            _books = books ?? throw new ArgumentNullException(nameof(books));
        }

        [HttpGet]
        [BooksResultFilter]
        public IActionResult GetBooks()
        {
            return Ok(_books.GetBooks());
        }
    }
}
