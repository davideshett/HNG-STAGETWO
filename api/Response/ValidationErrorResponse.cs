using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Response
{
    public class ValidationErrorResponse
    {
        public ICollection<ValidationError> Errors { get; set; }
    }
}