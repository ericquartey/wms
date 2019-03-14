using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces
{
    public interface IMainWindowBackToOAPPButtonViewModel : IViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion

        #region Methods

        void FinalizeBottomButtons();

        void InitializeBottomButtons();

        #endregion
    }
}
