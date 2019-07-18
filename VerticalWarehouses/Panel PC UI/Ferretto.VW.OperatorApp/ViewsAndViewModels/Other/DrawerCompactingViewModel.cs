using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class DrawerCompactingViewModel : BaseViewModel, IDrawerCompactingViewModel
    {
        #region Constructors

        public DrawerCompactingViewModel()
        {
            this.NavigationViewModel = null;
        }

        #endregion
    }
}
