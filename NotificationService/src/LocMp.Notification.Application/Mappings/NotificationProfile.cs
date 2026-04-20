using AutoMapper;
using LocMp.Notification.Application.DTOs;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Application.Mappings;

public sealed class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationEntity, NotificationDto>();
    }
}
