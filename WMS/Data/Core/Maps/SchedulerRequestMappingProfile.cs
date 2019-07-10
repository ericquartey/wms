using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class SchedulerRequestMappingProfile : Profile
    {
        #region Constructors

        public SchedulerRequestMappingProfile()
        {
            this.CreateMap<Common.DataModels.OperationType, OperationType>()
              .ConvertUsing(value => (OperationType)value);

            this.CreateMap<Common.DataModels.SchedulerRequestType, SchedulerRequestType>()
             .ConvertUsing(value => (SchedulerRequestType)value);

            this.CreateMap<Common.DataModels.SchedulerRequestStatus, SchedulerRequestStatus>()
             .ConvertUsing(value => (SchedulerRequestStatus)value);

            this.CreateMap<Common.DataModels.SchedulerRequest, SchedulerRequest>()
                .ForMember(s => s.MeasureUnitDescription, c => c.MapFrom(s => s.Item.MeasureUnit.Description));
        }

        #endregion
    }
}
