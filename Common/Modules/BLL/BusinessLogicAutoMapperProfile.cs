using AutoMapper;

namespace Ferretto.Common.Modules.BLL
{
  public class BusinessLogicAutoMapperProfile : Profile
  {
    public BusinessLogicAutoMapperProfile()
    {
      this.CreateMap<Common.Models.Item, Common.BLL.Interfaces.Models.IItem>().As<Models.Item>();
      this.CreateMap<Models.Item, Common.Models.Item>();
    }
  }
}
