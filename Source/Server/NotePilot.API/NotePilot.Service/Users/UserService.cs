using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotePilot.Entity.Models.User;
using NotePilot.Entity.User;
using NotePilot.Repository;
using NotePilot.Repository.Users;
using NotePilot.Service;

namespace NotePilot.Service.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly NotePilotDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly UserInfo _userInfo;

        public UserService(
            IUserRepository userRepository,
            NotePilotDbContext dbContext,
            IMapper mapper,
            ILogger<UserService> logger,
            UserInfo userInfo)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _userInfo = userInfo;
        }

        public async Task<UserRegistrationResponseModel> RegisterUserAsync(UserRegisterRequestModel request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("A user with this email already exists.");

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var userAuth = new UserAuthEntity
                    {
                        Id = Guid.NewGuid(),
                        UserName = request.UserName,
                        Email = request.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                        CreatedBy = _userInfo?.Username ?? "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    var createdAuth = await _userRepository.CreateUserAuthAsync(userAuth);

                    var userProfile = new UserProfileEntity
                    {
                        Id = Guid.NewGuid(),
                        UserAuthId = createdAuth.Id,
                        FullName = request.FullName,
                        PhoneNumber = request.PhoneNumber,
                        CreatedBy = _userInfo?.Username ?? "System",
                        CreatedDate = DateTime.UtcNow
                    };
                    await _userRepository.CreateUserProfileAsync(userProfile);

                    await transaction.CommitAsync();

                    return new UserRegistrationResponseModel
                    {
                        UserId = createdAuth.Id,
                        UserName = createdAuth.UserName,
                        Email = createdAuth.Email,
                        FullName = userProfile.FullName
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Registration failed for {Email}. Transaction rolled back.", request.Email);
                    throw; 
                }
            });
        }
    }
}