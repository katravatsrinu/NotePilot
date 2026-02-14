using Microsoft.AspNetCore.Http;
using NotePilot.Service;

namespace NotePilot.API.Middleware
{
    public class UserInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public UserInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserInfo userInfo)
        {
            var userIdClaim = context.User?.FindFirst("UserId")?.Value;
            var username = context.User?.Identity?.Name;
            var email = context.User?.FindFirst("Email")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                userInfo.UserId = userId;
            }

            userInfo.Username = username ?? "System";
            userInfo.Email = email;

            await _next(context);
        }
    }
}