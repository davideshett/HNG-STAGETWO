using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
         private readonly IUserRepository userRepository;

        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var dataFromRepo = await userRepository.GetUserById(userId);
            if (dataFromRepo == null)
            {
                return Unauthorized(new {
                    status = "Error",
                    Message = "You do not have permission to access this user",
                    StatusCode = 401
                });
            }

            return Ok(new {
                status = "Success",
                Message = $"Returned data for {dataFromRepo.UserForReturn.Email}",
                Data = dataFromRepo.UserForReturn
            });
        }
    }
}