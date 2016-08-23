using AutoMapper;
using Benefit.Domain.Models;
using Benefit.Web.Models.Admin;

namespace Benefit.Web.App_Start
{
    public class Automapper
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<EditUserViewModel, ApplicationUser>();
            });
        }
    }
}