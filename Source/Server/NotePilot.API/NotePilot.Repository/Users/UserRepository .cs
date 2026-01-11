using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotePilot.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePilot.Repository.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly NotePilotDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(NotePilotDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserAuthEntity> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.UserAuth
                    .Where(u => u.Email == email && u.DeletedDate == null)
                    .FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error fetching user by email: {Email}", email);
                return null;
            }
        }
        public async Task<UserAuthEntity> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _context.UserAuth
                    .Where(u => u.UserName == username && u.DeletedDate == null)
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error fetching user by username: {Username}", username);
                return null;
            }
        }
        public async Task<UserAuthEntity> CreateUserAuthAsync(UserAuthEntity userAuth)
        {
            try
            {
                await _context.Set<UserAuthEntity>().AddAsync(userAuth);
                await _context.SaveChangesAsync();

                return userAuth;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating user auth for email: {Email}", userAuth.Email);
                throw;
            }
        }

        public async Task<UserProfileEntity> CreateUserProfileAsync(UserProfileEntity userProfile)
        {
            try
            {
                await _context.Set<UserProfileEntity>().AddAsync(userProfile);
                await _context.SaveChangesAsync();

                return userProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating user profile for UserAuthId: {UserAuthId}", userProfile.UserAuthId);
                throw;
            }
        }

        public async Task<(UserAuthEntity UserAuth, UserProfileEntity UserProfile)> GetUserWithProfileAsync(Guid userAuthId)
        {
            try
            {
                var userAuth = await _context.Set<UserAuthEntity>()
                    .Where(u => u.Id == userAuthId && u.DeletedDate == null)
                    .FirstOrDefaultAsync();

                if (userAuth == null)
                {
                    return (null, null);
                }

                var userProfile = await _context.Set<UserProfileEntity>()
                    .Where(p => p.UserAuthId == userAuthId && p.DeletedDate == null)
                    .FirstOrDefaultAsync();

                return (userAuth, userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error fetching user with profile for UserAuthId: {UserAuthId}", userAuthId);
                throw;
            }
        }
    }
}