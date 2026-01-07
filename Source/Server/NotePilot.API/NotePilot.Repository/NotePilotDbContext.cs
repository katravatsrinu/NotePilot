using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotePilot.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace NotePilot.Repository
{
    public class NotePilotDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _disableAudit = false;

        public NotePilotDbContext(
            DbContextOptions<NotePilotDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #region DbSets

        public virtual DbSet<AuditRecordEntity> AuditRecords { get; set; }

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
            base.SaveChanges(true);
        }

        protected virtual List<AuditRecordEntity> AuditPreSave()
        {
            var currentUserId = GetCurrentUserId();
            var currentUsername = GetCurrentUsername();
            var currentTime = DateTime.UtcNow;

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
                    switch (change.State)
                    {
                        case EntityState.Added:
                            auditRecords.Add(new AuditRecordEntity
                            {
                                AuditType = AuditRecordEntity.AuditTypeEnum.Add,
                                Table = tableName,
                                PrimaryKey = null,
                                UserId = currentUserId,
                                CreatedBy = currentUsername,
                                CreatedDate = baseEntity?.CreatedDate ?? currentTime,
                                Entity = entity
                            });
                            break;

                        case EntityState.Deleted:
                            auditRecords.Add(new AuditRecordEntity
                            {
                                AuditType = AuditRecordEntity.AuditTypeEnum.Delete,
                                Table = tableName,
                                PrimaryKey = baseEntity?.Id.ToString(),
                                UserId = currentUserId,
                                CreatedBy = currentUsername,
                                CreatedDate = currentTime,
                                Entity = entity
                            });
                            break;

                        case EntityState.Modified:
                            auditRecords.AddRange(HandleModifiedEntity(
                                change, entity, baseEntity, tableName,
                                currentUserId, currentUsername, currentTime));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Audit error for {tableName}: {ex.Message}");
                }
            }

            return auditRecords;
        }

        private List<AuditRecordEntity> HandleModifiedEntity(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry change,
            object entity,
            BaseEntity baseEntity,
            string tableName,
            Guid? currentUserId,
            string currentUsername,
            DateTime currentTime)
        {
            var auditRecords = new List<AuditRecordEntity>();
            var originalValues = change.OriginalValues;
            var currentValues = change.CurrentValues;

            if (currentValues.TryGetValue(nameof(BaseEntity.DeletedDate), out object currentDeletedObj) &&
                currentDeletedObj is DateTime currentDeleted &&
                originalValues.TryGetValue(nameof(BaseEntity.DeletedDate), out object originalDeletedObj) &&
                originalDeletedObj == null)
            {
                auditRecords.Add(new AuditRecordEntity
                {
                    AuditType = AuditRecordEntity.AuditTypeEnum.SoftDelete,
                    Table = tableName,
                    Field = nameof(BaseEntity.DeletedDate),
                    PrimaryKey = baseEntity?.Id.ToString(),
                    OldValue = null,
                    NewValue = currentDeleted.ToString("O"),
                    UserId = currentUserId,
                    CreatedBy = currentUsername,
                    CreatedDate = currentDeleted,
                    Entity = entity
                });
            }

            foreach (var property in originalValues.Properties)
            {
                if (baseEntity != null && BaseEntityProperties.Contains(property.Name))
                    continue;

                var original = originalValues[property];
                var current = currentValues[property];

                if (!Equals(original, current))
                {
                    auditRecords.Add(new AuditRecordEntity
                    {
                        AuditType = AuditRecordEntity.AuditTypeEnum.Update,
                        Table = tableName,
                        Field = property.Name,
                        PrimaryKey = baseEntity?.Id.ToString(),
                        OldValue = original?.ToString(),
                        NewValue = current?.ToString(),
                        UserId = currentUserId,
                        CreatedBy = currentUsername,
                        CreatedDate = baseEntity?.UpdatedDate ?? currentTime,
                        Entity = entity
                    });
                }
            }

            return auditRecords;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("UserId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string GetCurrentUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? "System";
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