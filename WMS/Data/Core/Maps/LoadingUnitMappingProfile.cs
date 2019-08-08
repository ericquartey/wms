using AutoMapper;
using Ferretto.WMS.Data.Core.Models;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class LoadingUnitMappingProfile : Profile
    {
        #region Constructors

        public LoadingUnitMappingProfile()
        {
            this.CreateMap<LoadingUnitDetails, Common.DataModels.LoadingUnit>();
        }

        #endregion
    }
}
