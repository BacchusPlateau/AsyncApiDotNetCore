using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncAPIDotNetCore.Models
{
    public class BookCover
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
    }
}
