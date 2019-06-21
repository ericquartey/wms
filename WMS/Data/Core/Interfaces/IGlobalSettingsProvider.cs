using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Providers;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IGlobalSettingsProvider
    {
        #region Methods

        Task<GlobalSettings> GetGlobalSettingsAsync();

        #endregion
    }
}
