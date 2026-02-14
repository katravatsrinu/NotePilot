using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePilot.Entity.User
{
    [Table("UserAuth", Schema = "Auth")]
    public class UserAuthEntity : BaseEntity
    {

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } 

        [Required]
        [MaxLength(500)]
        public string Password { get; set; } 

    }
}
