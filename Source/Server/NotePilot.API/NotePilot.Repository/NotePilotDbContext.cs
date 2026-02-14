using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotePilot.Entity;
using NotePilot.Entity.User;
using NotePilot.Service;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace NotePilot.Repository
{
    public class NotePilotDbContext : DbContext
    {
        protected UserInfo _userInfo;
        protected readonly IConfiguration Configuration;
        private bool _disableAudit = false;

        public NotePilotDbContext(
            DbContextOptions<NotePilotDbContext> options,
            IConfiguration configuration,
            UserInfo userInfo) : base(options)
        {
            Configuration = configuration;
            _userInfo = userInfo;
        }

        public NotePilotDbContext(
            DbContextOptions<NotePilotDbContext> options,
            IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        #region User & Audit DbSets
        public virtual DbSet<AuditRecordEntity> AuditRecords { get; set; }
        public virtual DbSet<UserAuthEntity> UserAuth { get; set; }
        public virtual DbSet<UserProfileEntity> UserProfiles { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditRecordEntity>(entity =>
            {
                entity.ToTable("AuditRecord", "Audit");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OldValue).HasMaxLength(1000);
                entity.Property(e => e.NewValue).HasMaxLength(1000);
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AuditRecord_UserId");
                entity.Ignore(e => e.Entity);
            });
        }

        #region Audit Tracking

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (_disableAudit)
                return base.SaveChanges(acceptAllChangesOnSuccess);

            var auditRecords = AuditPreSave();
            var changeCount = base.SaveChanges(acceptAllChangesOnSuccess);
            AuditPostSave(auditRecords);

            return changeCount;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            if (_disableAudit)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            var auditRecords = AuditPreSave();
            var changeCount = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            AuditPostSave(auditRecords);

            return changeCount;
        }

        protected static readonly HashSet<string> BaseEntityProperties = new HashSet<string>(
            typeof(BaseEntity)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => x.Name));

        protected virtual void AuditPostSave(List<AuditRecordEntity> auditRecords)
        {
            foreach (var auditRecord in auditRecords
                .Where(x => x.AuditType == AuditRecordEntity.AuditTypeEnum.Add))
            {
                if (auditRecord.Entity is BaseEntity baseEntity)
                {
                    auditRecord.PrimaryKey = baseEntity.Id.ToString();
                }
            }

            AuditRecords.AddRange(auditRecords);
            var changeCount = base.SaveChanges(true);
        }

        protected virtual List<AuditRecordEntity> AuditPreSave()
        {
            DateTime auditCreatedDate(object? entity2)
            {
                var baseEntity = entity2 as BaseEntity;
                return baseEntity?.UpdatedDate
                       ?? baseEntity?.CreatedDate
                       ?? DateTime.UtcNow;
            }

            Guid? auditUserId() => _userInfo?.UserId;

            string auditCreatedBy(object entity)
            {
                var baseEntity = entity as BaseEntity;
                return _userInfo?.Username
                       ?? baseEntity?.UpdatedBy
                       ?? baseEntity?.CreatedBy
                       ?? "System";
            }

            string auditPrimaryKey(object? entity) => (entity as BaseEntity)?.Id.ToString();

            var auditRecords = new List<AuditRecordEntity>();

            foreach (var change in ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged))
            {
                var entity = change.Entity;
                var baseEntity = entity as BaseEntity;

                var tableName = entity.GetType().GetCustomAttribute<TableAttribute>()?.Name
                    ?? entity.GetType().Name;

                try
                {
                    if (change.State == EntityState.Added)
                    {
                        var auditRecord = new AuditRecordEntity();
                        if (auditRecord != null)
                        {
                            auditRecord.AuditType = AuditRecordEntity.AuditTypeEnum.Add;
                            auditRecord.Table = tableName;
                            auditRecord.PrimaryKey = null;
                            auditRecord.UserId = auditUserId();
                            auditRecord.CreatedBy = auditCreatedBy(baseEntity);
                            auditRecord.CreatedDate = auditCreatedDate(baseEntity);
                            auditRecord.Entity = entity;

                            auditRecords.Add(auditRecord);
                        }
                    }
                    else if (change.State == EntityState.Deleted)
                    {
                        var auditRecord = new AuditRecordEntity
                        {
                            AuditType = AuditRecordEntity.AuditTypeEnum.Delete,
                            Table = tableName,
                            PrimaryKey = auditPrimaryKey(baseEntity),
                            UserId = auditUserId(),
                            CreatedBy = auditCreatedBy(baseEntity),
                            CreatedDate = auditCreatedDate(baseEntity),
                            Entity = entity
                        };

                        auditRecords.Add(auditRecord);
                    }
                    else if (change.State == EntityState.Modified)
                    {
                        var originalValues = Entry(entity).OriginalValues;
                        var currentValues = Entry(entity).CurrentValues;

                        if (currentValues.TryGetValue(nameof(BaseEntity.DeletedDate), out DateTime? currentDeleted)
                            && currentDeleted.HasValue
                            && originalValues.TryGetValue(nameof(BaseEntity.DeletedDate), out DateTime? originalDeleted)
                            && !originalDeleted.HasValue)
                        {
                            var auditRecord = new AuditRecordEntity
                            {
                                AuditType = AuditRecordEntity.AuditTypeEnum.SoftDelete,
                                Table = tableName,
                                Field = nameof(BaseEntity.DeletedDate),
                                PrimaryKey = auditPrimaryKey(baseEntity),
                                OldValue = null,
                                NewValue = null,
                                UserId = auditUserId(),
                                Entity = entity,
                                CreatedBy = _userInfo?.Username ?? "System",
                                CreatedDate = Entry(entity).CurrentValues.GetValue<DateTime?>(nameof(BaseEntity.DeletedDate)) ?? DateTime.UtcNow
                            };

                            auditRecords.Add(auditRecord);
                        }

                        foreach (var property in originalValues.Properties)
                        {
                            if (baseEntity != null && BaseEntityProperties.Contains(property.Name))
                            {
                                continue;
                            }

                            var original = originalValues[property];
                            var current = currentValues[property];

                            if (!Equals(original, current))
                            {
                                var auditRecord = new AuditRecordEntity
                                {
                                    AuditType = AuditRecordEntity.AuditTypeEnum.Update,
                                    Table = tableName,
                                    Field = property.Name,
                                    PrimaryKey = auditPrimaryKey(baseEntity) ??
                                        (entity.GetType().GetProperty("Id") != null && entity.GetType().GetProperty("Id").PropertyType == typeof(Guid) ?
                                        entity.GetType().GetProperty("Id").GetValue(entity)?.ToString() : null),
                                    OldValue = original?.ToString(),
                                    NewValue = current?.ToString(),
                                    UserId = auditUserId(),
                                    Entity = entity,
                                    CreatedBy = auditCreatedBy(baseEntity),
                                    CreatedDate = auditCreatedDate(baseEntity)
                                };

                                auditRecords.Add(auditRecord);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return auditRecords;
        }

        #endregion

        #region SaveChanges Without Audit

        public int SaveChangesWithoutAudit()
        {
            try
            {
                _disableAudit = true;
                return base.SaveChanges();
            }
            finally
            {
                _disableAudit = false;
            }
        }

        public async Task<int> SaveChangesAsyncWithoutAudit()
        {
            try
            {
                _disableAudit = true;
                return await base.SaveChangesAsync();
            }
            finally
            {
                _disableAudit = false;
            }
        }

        #endregion
    }
}