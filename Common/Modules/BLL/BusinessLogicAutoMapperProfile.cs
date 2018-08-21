using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Ferretto.Common.Modules.BLL
{
  public class BusinessLogicAutoMapperProfile : Profile
  {
    public BusinessLogicAutoMapperProfile()
    {
      this.CreateMap<Common.Models.Item, Models.Item>();
      this.CreateMap<Models.Item, Common.Models.Item>();
    }
  }
}
