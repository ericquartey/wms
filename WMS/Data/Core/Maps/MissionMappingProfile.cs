using System.Linq;
using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class MissionMappingProfile : Profile
    {
        #region Constructors

        public MissionMappingProfile()
        {
            this.CreateMap<Common.DataModels.MissionStatus, MissionStatus>()
               .ConvertUsing(value => (MissionStatus)value);

            this.CreateMap<Common.DataModels.LoadingUnit, LoadingUnitMissionInfo>()
                .ForMember(l => l.Width, c => c.MapFrom(l => l.LoadingUnitType.LoadingUnitSizeClass.Width))
                .ForMember(l => l.Length, c => c.MapFrom(l => l.LoadingUnitType.LoadingUnitSizeClass.Length))
                .ForMember(l => l.Compartments, c => c.MapFrom(l => l.Compartments));

            this.CreateMap<Common.DataModels.Compartment, CompartmentMissionInfo>()
                .ForMember(cmp => cmp.Width, conf => conf.MapFrom(c => c.HasRotation ? c.CompartmentType.Height : c.CompartmentType.Width))
                .ForMember(cmp => cmp.Height, conf => conf.MapFrom(c => c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Height))
                .ForMember(
                    cmp => cmp.MaxCapacity,
                    conf => conf.MapFrom(
                        c => c.ItemId.HasValue ? c.CompartmentType.ItemsCompartmentTypes.SingleOrDefault(ict => ict.ItemId == c.ItemId).MaxCapacity : 0));

            this.CreateMap<Common.DataModels.Mission, Mission>()
                .ForMember(m => m.CompletedOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Completed)))
                .ForMember(m => m.ExecutingOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Executing)))
                .ForMember(m => m.IncompleteOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Incomplete)))
                .ForMember(m => m.NewOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.New)))
                .ForMember(m => m.OperationsCount, c => c.MapFrom(m => m.Operations.Count()))
                .ForMember(m => m.ErrorOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Error)));
            this.CreateMap<Mission, Common.DataModels.Mission>();

            this.CreateMap<Common.DataModels.Mission, MissionInfo>()
                                .ForMember(m => m.CompletedOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Completed)))
                .ForMember(m => m.ExecutingOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Executing)))
                .ForMember(m => m.IncompleteOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Incomplete)))
                .ForMember(m => m.NewOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.New)))
                .ForMember(m => m.OperationsCount, c => c.MapFrom(m => m.Operations.Count()))
                .ForMember(m => m.ErrorOperationsCount, c => c.MapFrom(m => m.Operations.Count(o => o.Status == Common.DataModels.MissionOperationStatus.Error)));

            this.CreateMap<Common.DataModels.Mission, MissionWithLoadingUnitDetails>()
                .ForMember(m => m.Operations, c => c.MapFrom(m => m.Operations));
        }

        #endregion
    }
}
