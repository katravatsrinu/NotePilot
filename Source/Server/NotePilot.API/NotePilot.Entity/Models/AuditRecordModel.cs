using AutoMapper;
using NotePilot.Entity;
using static NotePilot.Entity.AuditRecordEntity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NotePilot.Entity.Models
{
    public class AuditRecordModel
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Table { get; set; }
        public string Field { get; set; } = null;
        public string OldValue { get; set; } = null;
        public string NewValue { get; set; } = null;
        public AuditTypeEnum AuditType { get; set; }
        public string CreatedBy { get; set; } = null;
        public DateTime CreatedDate { get; set; }
    }

    public class AuditRecordModelProfile : Profile
    {
        public AuditRecordModelProfile()
        {
            CreateMap<AuditRecordEntity, AuditRecordModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                .ForMember(dest => dest.Table, opt => opt.MapFrom(src => src.Table))
                .ForMember(dest => dest.OldValue, opt => opt.MapFrom(src => src.OldValue))
                .ForMember(dest => dest.NewValue, opt => opt.MapFrom(src => src.NewValue))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.AuditType));
        }
    }
}
