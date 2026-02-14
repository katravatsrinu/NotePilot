using NotePilot.Entity.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePilot.Service.Users
{
    public interface IUserService
    {
        Task<UserRegistrationResponseModel> RegisterUserAsync(UserRegisterRequestModel request);
    }
}
