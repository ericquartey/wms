using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public interface ILSMTMainViewModel : IViewModel
    {
        #region Properties

        BindableBase LSMTContentRegionCurrentViewModel { get; set; }

        #endregion
    }
}
