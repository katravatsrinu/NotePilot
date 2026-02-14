using System.Diagnostics.CodeAnalysis;

namespace NotePilot.Service
{
    [ExcludeFromCodeCoverage]
    public class UserInfo
    {
        public string Username { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
    }
}