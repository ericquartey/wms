using AutoMapper;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class MachineStatusMappingProfile : Profile
    {
        #region Constructors

        public MachineStatusMappingProfile()
        {
            this.CreateMap<VW.MachineAutomationService.Hubs.MachineStatus, Data.Hubs.Models.MachineStatus>();
            this.CreateMap<VW.MachineAutomationService.Hubs.BayStatus, Data.Hubs.Models.BayStatus>();
            this.CreateMap<VW.MachineAutomationService.Hubs.ElevatorStatus, Data.Hubs.Models.ElevatorStatus>();
            this.CreateMap<VW.MachineAutomationService.Hubs.MachineMode, Data.Hubs.Models.MachineMode>()
                 .ConvertUsing(value => (Data.Hubs.Models.MachineMode)value);
        }

        #endregion
    }
}
