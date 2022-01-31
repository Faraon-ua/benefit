using AutoMapper;
using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using Benefit.Services;
using Benefit.Web.Models.Admin;
using System;

namespace Benefit.RestApi.App_Start
{
    public class AutomapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<DataTransfer.ApiDto.Allo.OrderDto, Order>()
                    .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.note.Truncate(256)))
                    .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTime.Parse(src.created_date)))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SettingsService.Allo.OrderStatusMapping[src.status.status]))
                    .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.shipping.type))
                    .ForMember(dest => dest.ShippingTrackingNumber, opt => opt.MapFrom(src => src.shipping.tracking_number))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => string.Format("{0} {1}", src.customer.firstname, src.customer.lastname)))
                    .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.customer.telephone));
                cfg.CreateMap<DataTransfer.ApiDto.Rozetka.OrderDto, Order>()
                    .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.comment))
                    .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTime.Parse(src.created)))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (OrderStatus)src.status - 1))
                    .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.delivery.delivery_service_name))
                    .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.delivery.cost.GetValueOrDefault(0)))
                    .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => string.Format("{0}, {1} {2}", src.delivery.city.title, src.delivery.place_street, src.delivery.place_house + " " + src.delivery.place_flat)))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.delivery.recipient_title))
                    .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.user_phone));
                cfg.CreateMap<DataTransfer.ApiDto.Allo.OrderProductDto, OrderProduct>()
                    .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.sku))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.name))
                    .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.price))
                    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.quantity));
                cfg.CreateMap<DataTransfer.ApiDto.Rozetka.OrderProductDto, OrderProduct>()
                    .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.id))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.item_name))
                    .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.price))
                    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.quantity));
            });
        }
    }
}