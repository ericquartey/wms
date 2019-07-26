using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class ItemAreaMappingProfile : Profile
    {
        #region Constructors

        public ItemAreaMappingProfile()
        {
            this.CreateMap<ItemArea, Common.DataModels.ItemArea>();
        }

        #endregion
    }
}
