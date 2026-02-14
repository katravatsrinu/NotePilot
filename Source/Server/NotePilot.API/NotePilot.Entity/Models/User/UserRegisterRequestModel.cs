using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePilot.Entity.Models.User
{
    public class UserRegisterRequestModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
