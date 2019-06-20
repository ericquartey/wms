using System;
using System.Collections.Generic;
using System.Text;
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
