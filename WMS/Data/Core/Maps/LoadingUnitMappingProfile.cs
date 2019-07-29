using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class LoadingUnitMappingProfile : Profile
    {
        #region Constructors

        public LoadingUnitMappingProfile()
        {
            this.CreateMap<ReferenceType, Common.DataModels.ReferenceType>()
                .ConvertUsing(value => (Common.DataModels.ReferenceType)value);

            this.CreateMap<LoadingUnitDetails, Common.DataModels.LoadingUnit>();
        }

        #endregion
    }
}
