using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Response
{
    public class GenericResponsee
    {
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
        public int StatusCode { get; set; }
    }
}