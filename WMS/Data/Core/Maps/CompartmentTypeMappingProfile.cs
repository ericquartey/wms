using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class CompartmentTypeMappingProfile : Profile
    {
        #region Constructors

        public CompartmentTypeMappingProfile()
        {
            this.CreateMap<CompartmentType, Common.DataModels.CompartmentType>();
        }

        #endregion
    }
}
