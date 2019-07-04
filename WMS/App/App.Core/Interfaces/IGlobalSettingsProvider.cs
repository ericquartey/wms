using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IGlobalSettingsProvider
    {
        #region Methods

        Task<GlobalSettings> GetAllAsync();

        #endregion
    }
}
