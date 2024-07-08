using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.User;
using api.Models;
using api.Response;

namespace api.Repo
{
    public interface IUserRepository
    {
        Task<RegistrationResponse> Register (UserRegistrationDto registrationDto);
        Task<LoginResponse> Login(LoginDto model);
        Task<bool> UserExists(string email);
        bool UserExistsUid(string userId);
        Task<GetUserByIdResponse> GetUserById(string userId);
        bool IsInSameOrganisation(User LoggedInUser, string userId);
    }
}