using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class ItemsCompartmentTypeMappingProfile : Profile
    {
        #region Constructors

        public ItemsCompartmentTypeMappingProfile()
        {
            this.CreateMap<ItemCompartmentType, Common.DataModels.ItemCompartmentType>();
        }

        #endregion
    }
}
