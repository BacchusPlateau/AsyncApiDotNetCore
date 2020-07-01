using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAPIDotNetCore.Filters;
using AsyncAPIDotNetCore.ModelBinders;
using AsyncAPIDotNetCore.Models;
using AsyncAPIDotNetCore.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAPIDotNetCore.Controllers
{
    [ApiController]
    [Route("api/bookcollections")]
    [BooksResultFilter]
    public class BookCollectionsController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;
        public BookCollectionsController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookCollection(IEnumerable<BookForCreation> bookCollection)
        {
            var books = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);

            foreach(var book in books)
            {
                _booksRepository.AddBook(book);
            }

            await _booksRepository.SaveChangesAsync();

            var booksToReturn = await _booksRepository.GetBooksAsync(
                books.Select(b => b.Id).ToList());

            var bookIds = string.Join(",", booksToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetBookCollection", new { bookIds }, booksToReturn);
        }

        [HttpGet("({bookIds})", Name = "GetBookCollection")]  //api/bookcollections/[id1,id2,...]
        public async Task<IActionResult> GetBookCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> bookIds)
        {
            var books = await _booksRepository.GetBooksAsync(bookIds);

            if (books.Count() == bookIds.Count())
            {
                return Ok(books);
            }

            return NotFound();
        }

    }
}
