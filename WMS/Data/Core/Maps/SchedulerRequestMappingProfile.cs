using AutoMapper;
using Ferretto.WMS.Data.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class SchedulerRequestMappingProfile : Profile
    {
        #region Constructors

        public SchedulerRequestMappingProfile()
        {
            this.CreateMap<Common.DataModels.SchedulerRequest, SchedulerRequest>()
                .ForMember(s => s.MeasureUnitDescription, c => c.MapFrom(s => s.Item.MeasureUnit.Description));
        }

        #endregion
    }
}
