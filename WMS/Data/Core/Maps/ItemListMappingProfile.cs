using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class ItemListMappingProfile : Profile
    {
        #region Constructors

        public ItemListMappingProfile()
        {
            this.CreateMap<Common.DataModels.ItemListType, ItemListType>()
                .ConvertUsing(value => (ItemListType)value);

            this.CreateMap<Common.DataModels.ItemListRowStatus, ItemListRowStatus>()
                .ConvertUsing(value => (ItemListRowStatus)value);

            this.CreateMap<Common.DataModels.ItemList, ItemList>();
            this.CreateMap<Common.DataModels.ItemListRow, ItemListRow>();
        }

        #endregion
    }
}
