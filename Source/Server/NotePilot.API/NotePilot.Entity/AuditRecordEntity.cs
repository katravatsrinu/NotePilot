using NotePilot.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace NotePilot.Entity
{
    [Table("AuditRecord", Schema = "Audit")]
    [ExcludeFromCodeCoverage]
    public class AuditRecordEntity : BaseEntity
    {
        public const int MAX_VALUE_LENGTH = 1000;
        private string _oldValue;
        private string _newValue;

        public Guid? UserId { get; set; }
        public AuditTypeEnum AuditType { get; set; }

        [Column("Table")]
        public string Table { get; set; }

        [Column("Field")]
        public string Field { get; set; }

        [Column("PrimaryKey")]
        public string PrimaryKey { get; set; }

        [MaxLength(MAX_VALUE_LENGTH)]
        public string OldValue
        {
            get => _oldValue;
            set => _oldValue = value?.Length > MAX_VALUE_LENGTH ? value.Substring(0, MAX_VALUE_LENGTH) : value;
        }

        [MaxLength(MAX_VALUE_LENGTH)]
        public string NewValue
        {
            get => _newValue;
            set => _newValue = value?.Length > MAX_VALUE_LENGTH ? value.Substring(0, MAX_VALUE_LENGTH) : value;
        }

        [NotMapped]
        public object Entity { get; set; }

        public enum AuditTypeEnum
        {
            Unknown = 0,
            Add = 1,
            Update = 2,
            Delete = 3,
            SoftDelete = 4
        }
    }
}