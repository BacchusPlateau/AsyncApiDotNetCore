using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncAPIDotNetCore.Filters;
using AsyncAPIDotNetCore.Models;
using AsyncAPIDotNetCore.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAPIDotNetCore.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BooksController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [BooksResultFilter]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksRepository.GetBooksAync();
            return Ok(books);
        }

        [HttpGet]
        [Route("{id}", Name = "GetBook")]    //example:  https://localhost:44335/api/books/493c3228-3444-4a49-9cc0-e8532edc59b2
        [BookResultFilter]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);
            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        [BookResultFilter]
        public async Task<IActionResult> CreateBook(BookForCreation bookForCreation)
        {
            var book = _mapper.Map<Entities.Book>(bookForCreation);
            _booksRepository.AddBook(book);

            await _booksRepository.SaveChangesAsync();
            await _booksRepository.GetBookAsync(book.Id);  //loads author details into the context


            return CreatedAtRoute("GetBook", new { id = book.Id }, book);
        }
    }
}
