using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormPdfEasy.Models
{
    public class TokenDto
    {
        public string token { get; set; }
        public DateTime expiration { get; set; }
    }
}
