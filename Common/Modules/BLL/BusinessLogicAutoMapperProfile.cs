using AutoMapper;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL
{
    public class BusinessLogicAutoMapperProfile : Profile
    {
        #region Constructors

        public BusinessLogicAutoMapperProfile()
        {
            this.CreateMap<DataModels.Item, Item>().As<Models.Item>();
            this.CreateMap<Item, DataModels.Item>();
        }

        #endregion Constructors
    }
}
