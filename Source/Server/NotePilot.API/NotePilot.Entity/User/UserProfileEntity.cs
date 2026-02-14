using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotePilot.Entity.User
{
    [Table("UserProfile", Schema = "User")]
    public class UserProfileEntity : BaseEntity
    {
        [Required]
        public Guid UserAuthId { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [ForeignKey(nameof(UserAuthId))]
        public UserAuthEntity UserAuth { get; set; }
    }
}
