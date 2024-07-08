using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.User;

namespace api.Response
{
    public class LoginResponse
    {
        public bool IsSuccessful { get; set; }
        public string AccessToken { get; set; }
        public UserForReturn User { get; set; }
    }
}