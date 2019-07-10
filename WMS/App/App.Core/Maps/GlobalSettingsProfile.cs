using AutoMapper;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Maps
{
    public class GlobalSettingsProfile : Profile
    {
        #region Constructors

        public GlobalSettingsProfile()
        {
            this.CreateMap<GlobalSettings, Data.WebAPI.Contracts.GlobalSettings>();
            this.CreateMap<Data.WebAPI.Contracts.GlobalSettings, GlobalSettings>();
        }

        #endregion
    }
}
