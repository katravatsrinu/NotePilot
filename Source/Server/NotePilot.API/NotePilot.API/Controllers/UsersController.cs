using Microsoft.AspNetCore.Mvc;
using NotePilot.Entity.Models.User;
using NotePilot.Service.Users;

namespace NotePilot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestModel request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            try
            {
                var result = await _userService.RegisterUserAsync(request);

                return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration for {Email}", request.Email);

                return BadRequest(new { message = ex.Message });
            }
        }
    }
}