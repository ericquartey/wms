using AutoMapper;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class CompartmentMappingProfile : Profile
    {
        #region Fields

        public CompartmentMappingProfile()
        {
            this.CreateMap<CompartmentDetails, Common.DataModels.Compartment>();
        }

        #endregion
    }
}
