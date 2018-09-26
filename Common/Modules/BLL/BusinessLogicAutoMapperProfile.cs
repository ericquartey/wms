using AutoMapper;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL
{
    public class BusinessLogicAutoMapperProfile : Profile
    {
        #region Constructors

        public BusinessLogicAutoMapperProfile()
        {
            this.CreateMap<DataModels.Item, Item>();
            this.CreateMap<Item, DataModels.Item>();

            this.CreateMap<DataModels.Compartment, Compartment>();
            this.CreateMap<Compartment, DataModels.Compartment>();

            this.CreateMissingTypeMaps = true;
        }

        #endregion Constructors
    }
}
