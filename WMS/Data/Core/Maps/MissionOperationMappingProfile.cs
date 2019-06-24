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

            this.CreateMap<Common.DataModels.MissionOperation, MissionOperationInfo>();

            this.CreateMap<MissionOperation, Common.DataModels.MissionOperation>();
            this.CreateMap<Common.DataModels.MissionOperation, MissionOperation>()
                .ConstructUsing((m, context) => m.ItemListRowId.HasValue
                ? context.Mapper.Map<MissionListOperation>(m)
                : context.Mapper.Map<MissionOperation>(m));
        }

        #endregion
    }
}
