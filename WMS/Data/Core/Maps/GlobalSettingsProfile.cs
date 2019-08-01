using AutoMapper;

namespace Ferretto.WMS.Data.Core.Maps
{
    public class GlobalSettingsProfile : Profile
    {
        #region Constructors

        public GlobalSettingsProfile()
        {
            this.CreateMap<GlobalSettings, Common.DataModels.GlobalSettings>();
            this.CreateMap<Common.DataModels.GlobalSettings, GlobalSettings>();
        }

        #endregion
    }
}
