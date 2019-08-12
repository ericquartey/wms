using AutoMapper;
using Ferretto.WMS.Data.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class ItemListMappingProfile : Profile
    {
        #region Constructors

        public ItemListMappingProfile()
        {
            this.CreateMap<Common.DataModels.ItemList, ItemList>();
            this.CreateMap<Common.DataModels.ItemListRow, ItemListRow>();

            this.CreateMap<ItemListDetails, Common.DataModels.ItemList>();
            this.CreateMap<ItemListRowDetails, Common.DataModels.ItemListRow>();
        }

        #endregion
    }
}
