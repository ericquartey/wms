using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Simulator.Services.Models;

namespace Ferretto.VW.Simulator.Services.Interfaces
{
    public interface IThemeService
    {
        #region Properties

        ApplicationTheme ActiveTheme { get; }

        #endregion

        #region Methods

        void ApplyTheme(ApplicationTheme theme);

        #endregion
    }
}
