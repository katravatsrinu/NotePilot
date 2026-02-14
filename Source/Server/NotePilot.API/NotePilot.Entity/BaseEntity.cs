using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePilot.Entity
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(255)]
        public string UpdatedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

        [NotMapped]
        public bool IsDeleted => DeletedDate.HasValue;
    }
}
