using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IDrawerOperationsMainViewModel : IViewModel
    {
        #region Methods

        ICommand BackToMainWindowNavigationButtonsViewButtonCommand();

        void NavigateToView<T, I>()
            where T : BindableBase, I
            where I : IViewModel;

        #endregion
    }
}
