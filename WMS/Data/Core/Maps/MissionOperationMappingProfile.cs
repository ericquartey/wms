using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class MissionOperationMappingProfile : Profile
    {
        #region Constructors

        public MissionOperationMappingProfile()
        {
            this.CreateMap<Common.DataModels.MissionOperationType, MissionOperationType>()
               .ConvertUsing(value => (MissionOperationType)value);

            this.CreateMap<Common.DataModels.MissionOperationStatus, MissionOperationStatus>()
                .ConvertUsing(value => (MissionOperationStatus)value);

            this.CreateMap<Common.DataModels.Item, ItemMissionInfo>();
            this.CreateMap<Common.DataModels.ItemList, ItemListMissionInfo>();
            this.CreateMap<Common.DataModels.ItemListRow, ItemListRowMissionInfo>();

            this.CreateMap<Common.DataModels.MissionOperation, MissionOperationInfo>()
                .ForMember(
                    m => m.CompartmentWidth,
                    conf => conf.MapFrom(
                        m => m.Compartment.HasRotation
                            ? m.Compartment.CompartmentType.Depth
                            : m.Compartment.CompartmentType.Width))
                .ForMember(
                    m => m.CompartmentDepth,
                    conf => conf.MapFrom(
                        m => m.Compartment.HasRotation
                            ? m.Compartment.CompartmentType.Width
                            : m.Compartment.CompartmentType.Depth))
                .ForMember(m => m.ItemMeasureUnitDescription, conf => conf.MapFrom(m => m.Item.MeasureUnit.Description));

            this.CreateMap<MissionOperation, Common.DataModels.MissionOperation>();
            this.CreateMap<Common.DataModels.MissionOperation, MissionOperation>();
        }

        #endregion
    }
}
