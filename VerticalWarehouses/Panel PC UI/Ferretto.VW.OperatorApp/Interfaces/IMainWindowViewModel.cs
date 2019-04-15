using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IMainWindowViewModel
    {
        #region Properties

        BindableBase ContentRegionCurrentViewModel { get; set; }

        BindableBase ExitViewButtonRegionCurrentViewModel { get; set; }

        BindableBase NavigationRegionCurrentViewModel { get; set; }

        #endregion
    }
}
