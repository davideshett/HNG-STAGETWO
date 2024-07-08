using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.User;
using api.Repo;
using api.Response;
using api.Service;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenService tokenService;

        public AuthController(IUserRepository userRepository, ITokenService tokenService)
        {
            this.userRepository = userRepository;
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var dataFromRepo = await userRepository.Login(model);
            if (dataFromRepo == null)
            {
                return Unauthorized(new {
                    Status = "Bad request",
                    Message = "Authentication failed",
                    StatusCode = 401
                });
            }

            if(dataFromRepo.IsSuccessful)
            {
                return Ok(new {
                    Status = "success",
                    Message = "Login successful",
                    Data = new
                    {
                        accessToken = dataFromRepo.AccessToken,
                        user = dataFromRepo.User
                    }
                });
            }

            return BadRequest(new {
                    Status = "Bad request",
                    Message = "Authentication failed",
                    StatusCode = 400
                });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
        {
            var result = await userRepository.Register(userRegistrationDto);
            
            if (result.IsSuccessful)
            {
                var createdResult = new 
                {
                    Status = "success",
                    Message = "Registration successful",
                    Data = new
                    {
                        accessToken = result.AccessToken,
                        user = result.User
                    }
                };

                return Created("",createdResult);
            }

            if (result.Errors.Count > 0)
            {
                return UnprocessableEntity(new { result.Errors });
            }

             return BadRequest(new {
                    Status = "Bad request",
                    Message = "Registration unsuccessful",
                    StatusCode = 400
                });
        }
    }
}