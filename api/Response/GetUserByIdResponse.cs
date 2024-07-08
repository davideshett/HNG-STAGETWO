using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.User;

namespace api.Response
{
    public class GetUserByIdResponse
    {
        public bool UserNotFound { get; set; }
        public UserForReturn UserForReturn { get; set; }
        
    }
}