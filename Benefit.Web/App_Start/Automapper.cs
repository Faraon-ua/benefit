using AutoMapper;
using Benefit.Domain.Models;
using Benefit.Web.Models.Admin;

namespace Benefit.Web.App_Start
{
    public class AutomapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<EditUserViewModel, ApplicationUser>();
                cfg.CreateMap<OrderVM, Order>();
                cfg.CreateMap<Category, CategoryVM>();
            });
        }
    }
}