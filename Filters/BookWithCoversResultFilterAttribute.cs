using AsyncAPIDotNetCore.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncAPIDotNetCore.Filters
{
    public class BookWithCoversResultFilterAttribute : ResultFilterAttribute
    {

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {

            var resultFromAction = context.Result as ObjectResult;
            if (resultFromAction?.Value == null 
                || resultFromAction.StatusCode < 200
                || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            //deconstruct the result value from the controller action
            var (book, bookCovers) = ((Entities.Book, IEnumerable<ExternalModels.BookCover> bookCovers))resultFromAction.Value;

            //create instance of the mapper from context
            var mapper = context.HttpContext.RequestServices.GetRequiredService<IMapper>();

            //get a mapped BookWithCovers from a Book
            var mappedBook = mapper.Map<BookWithCovers>(book);

            //finally, include the book into the bookCovers
            resultFromAction.Value = mapper.Map(bookCovers, mappedBook);

            await next();

        }

    }
}
