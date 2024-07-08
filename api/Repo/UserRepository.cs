using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using api.Data;
using api.Dto.User;
using api.Models;
using api.Response;
using api.Service;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace api.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> userManager;
        private readonly DataContext dataContext;
        private readonly ITokenService tokenService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserRepository(UserManager<User> userManager, DataContext dataContext,
        ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
            this.dataContext = dataContext;
            this.tokenService = tokenService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<RegistrationResponse> Register(UserRegistrationDto registrationDto)
        {
            var errors = ReturnErrors(registrationDto);

            if (errors.Count > 0)
            {
                return new RegistrationResponse
                {
                    Errors = errors,
                    IsSuccessful = false
                };
            }

            var uiD = "";

            if(registrationDto.UserId == null)
            {
                uiD = Guid.NewGuid().ToString();
            }else
            {
                uiD = registrationDto.UserId;
            }

            var data = new User 
            {
                UserName = registrationDto.Email,
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.Phone,
                Phone = registrationDto.Phone,
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                UserId = uiD
            };

            var result = await userManager.CreateAsync(data, registrationDto.Password);

            if (result.Succeeded)
            {
                var org = new Organisation
                {
                    OrgId = Guid.NewGuid().ToString(),
                    Name = $"{data.FirstName}'s Organisation",
                    Description = "Description Here"
                };
                await dataContext.Organisations.AddAsync(org);

                var RegisteredUser = await userManager.Users
                .Include(X => X.Organisations)
                .FirstOrDefaultAsync(x => x.UserId.Equals(data.UserId));

                RegisteredUser.Organisations.Add(org);
                await dataContext.SaveChangesAsync();

                var user = new UserForReturn
                {
                    UserId = data.UserId,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email = data.Email,
                    Phone = data.Phone,
                };

                return new RegistrationResponse
                {
                    IsSuccessful = true,
                    AccessToken = await tokenService.CreateToken(data),
                    Errors = errors,
                    User = user
                };
            }


            foreach (var identityError in result.Errors)
            {
                errors.Add(new ValidationError
                {
                    Field = identityError.Code,
                    Message = identityError.Description
                });
            }

            return new RegistrationResponse
            {
                Errors = errors,
                IsSuccessful = false,
                User = null
            };

        }

        public async Task<bool> UserExists(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            return true;
        }

        public bool PhoneNumberIsValid(string input)
        {
            if (input.Length != 11)
            {
                return false;
            }

            if (!input.StartsWith('0'))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                return false; // Empty or whitespace-only strings are not valid numbers.
            }

            // Regex pattern to match only numbers.
            string pattern = @"^\d+$";
            return Regex.IsMatch(input, pattern);
        }


        List<ValidationError> ReturnErrors(UserRegistrationDto registrationDto)
        {
            var errors = new List<ValidationError>();

            if (!registrationDto.Email.Contains('@'))
            {
                errors.Add(new ValidationError
                {
                    Field = "Email",
                    Message = "Invalid email"
                });
            }

             if (UserExistsUid(registrationDto.UserId))
            {
                errors.Add(new ValidationError
                {
                    Field = "UserId",
                    Message = "User id must be unique"
                });
            }


            if (string.IsNullOrEmpty(registrationDto.Email))
            {
                errors.Add(new ValidationError
                {
                    Field = "Email",
                    Message = "Email required"
                });
            }

            if (string.IsNullOrEmpty(registrationDto.FirstName))
            {
                errors.Add(new ValidationError
                {
                    Field = "First Name",
                    Message = "First Name required"
                });
            }

            if (string.IsNullOrEmpty(registrationDto.LastName))
            {
                errors.Add(new ValidationError
                {
                    Field = "Last Name",
                    Message = "First Name required"
                });
            }

            if (string.IsNullOrEmpty(registrationDto.Phone))
            {
                errors.Add(new ValidationError
                {
                    Field = "Phone number",
                    Message = "Invalid Phone number"
                });
            }

            if (string.IsNullOrEmpty(registrationDto.Password))
            {
                errors.Add(new ValidationError
                {
                    Field = "Password",
                    Message = "Password can not be null"
                });
            }

            if (!PhoneNumberIsValid(registrationDto.Phone))
            {
                errors.Add(new ValidationError
                {
                    Field = "Phone number",
                    Message = "Error! Expectecd characters: 11 EG: 09024881134"
                });
            }

            return errors;
        }

        public async Task<LoginResponse> Login(LoginDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new LoginResponse
                {
                    IsSuccessful = false,
                    AccessToken = null,
                    User = null
                };
            }


            bool isValidUser = await userManager.CheckPasswordAsync(user, model.Password);

            if (isValidUser)
            {
                var token = await tokenService.CreateToken(user);
                var userForReturn = new UserForReturn
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.NormalizedEmail.ToLower(),
                    Phone = user.Phone,
                };

                return new LoginResponse
                {
                    IsSuccessful = true,
                    AccessToken = token,
                    User = userForReturn
                };

            }

            return new LoginResponse
            {
                IsSuccessful = false,
                AccessToken = null,
                User = null
            };
        }

        public async Task<GetUserByIdResponse> GetUserById(string userId)
        {
            var userForReturn = new UserForReturn { };

            var LoggedInUSerId = httpContextAccessor.HttpContext.User.FindFirstValue("userId");
            var LoggedInUser = await userManager.Users
            .Include(X => X.Organisations)
            .FirstOrDefaultAsync(x => x.UserId.Equals(LoggedInUSerId));

            if (LoggedInUSerId.Equals(userId))
            {
                return new GetUserByIdResponse
                {
                    UserNotFound = false,
                    UserForReturn = new UserForReturn
                    {
                        UserId = LoggedInUser.UserId,
                        FirstName = LoggedInUser.FirstName,
                        LastName = LoggedInUser.LastName,
                        Email = LoggedInUser.NormalizedEmail.ToLower(),
                        Phone = LoggedInUser.Phone
                    }
                };
            }

            var anotherUser = await userManager.Users
            .Include(X => X.Organisations)
            .FirstOrDefaultAsync(x => x.UserId.Equals(userId));

            if (anotherUser == null)
            {
                return null;
            }

            if (IsInSameOrganisation(LoggedInUser, userId))
            {
                userForReturn = new UserForReturn
                {
                    UserId = anotherUser.UserId,
                    FirstName = anotherUser.FirstName,
                    LastName = anotherUser.LastName,
                    Email = anotherUser.NormalizedEmail.ToLower(),
                    Phone = anotherUser.Phone
                };

                return new GetUserByIdResponse
                {
                    UserNotFound = false,
                    UserForReturn = userForReturn
                };
            }

            return null;
        }

        public bool IsInSameOrganisation(User LoggedInUser, string userId)
        {
            foreach (var org in LoggedInUser.Organisations)
            {
                foreach (var user in org.Users)
                {
                    if (user.UserId.Equals(userId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool UserExistsUid(string userId)
        {
            var user =  userManager.Users.FirstOrDefault(x=> x.UserId.Equals(userId));
            if (user == null)
            {
                return false;
            }

            return true;
        }
    }
}
