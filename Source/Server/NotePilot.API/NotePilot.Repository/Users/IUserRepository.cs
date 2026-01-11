using NotePilot.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePilot.Repository.Users
{
    public interface IUserRepository
    {
        Task<UserAuthEntity> GetUserByEmailAsync(string email);
        Task<UserAuthEntity> GetUserByUsernameAsync(string username);
        Task<UserAuthEntity> CreateUserAuthAsync(UserAuthEntity userAuth);
        Task<UserProfileEntity> CreateUserProfileAsync(UserProfileEntity userProfile);
        Task<(UserAuthEntity UserAuth, UserProfileEntity UserProfile)> GetUserWithProfileAsync(Guid userAuthId);
    }
}
