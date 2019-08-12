using System.Linq;
using AutoMapper;
using Ferretto.WMS.Data.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class ItemMappingProfile : Profile
    {
        #region Constructors

        public ItemMappingProfile()
        {
            this.CreateMap<ItemAvailable, Common.DataModels.Item>();
            this.CreateMap<Common.DataModels.Item, ItemAvailable>();

            this.CreateMap<Item, Common.DataModels.Item>();
            this.CreateMap<Common.DataModels.Item, Item>()
                .ForMember(i => i.CompartmentsCount, c => c.MapFrom(i => i.Compartments.Count()))
                .ForMember(i => i.MissionOperationsCount, c => c.MapFrom(i => i.MissionOperations.Count()))
                .ForMember(i => i.SchedulerRequestsCount, c => c.MapFrom(i => i.SchedulerRequests.Count()))
                .ForMember(i => i.ItemListRowsCount, c => c.MapFrom(i => i.ItemListRows.Count()))
                .ForMember(i => i.HasCompartmentTypes, c => c.MapFrom(i => i.ItemsCompartmentTypes.Any()))
                .ForMember(i => i.HasAssociatedAreas, c => c.MapFrom(i => i.ItemAreas.Any()))
                .ForMember(i => i.TotalStock, c => c.MapFrom(i => i.Compartments.Sum(cm => cm.Stock)))
                .ForMember(i => i.TotalReservedForPick, c => c.MapFrom(i => i.Compartments.Sum(cm => cm.ReservedForPick)))
                .ForMember(i => i.TotalReservedToPut, c => c.MapFrom(i => i.Compartments.Sum(cm => cm.ReservedToPut)))
                .ForMember(i => i.TotalAvailable, c => c.MapFrom(i => i.Compartments.Sum(cm => cm.Stock + cm.ReservedToPut - cm.ReservedForPick)));

            this.CreateMap<ItemDetails, Common.DataModels.Item>();
            this.CreateMap<Common.DataModels.Item, ItemDetails>()
                .ForMember(i => i.CompartmentsCount, c => c.MapFrom(i => i.Compartments.Count()))
                .ForMember(i => i.MissionOperationsCount, c => c.MapFrom(i => i.MissionOperations.Count()))
                .ForMember(i => i.SchedulerRequestsCount, c => c.MapFrom(i => i.SchedulerRequests.Count()))
                .ForMember(i => i.ItemListRowsCount, c => c.MapFrom(i => i.ItemListRows.Count()))
                .ForMember(i => i.HasCompartmentTypes, c => c.MapFrom(i => i.ItemsCompartmentTypes.Any()))
                .ForMember(i => i.HasAssociatedAreas, c => c.MapFrom(i => i.ItemAreas.Any()))
                .ForMember(i => i.TotalAvailable, c => c.MapFrom(i => i.Compartments.Sum(cm => cm.Stock + cm.ReservedToPut - cm.ReservedForPick)));
        }

        #endregion
    }
}
