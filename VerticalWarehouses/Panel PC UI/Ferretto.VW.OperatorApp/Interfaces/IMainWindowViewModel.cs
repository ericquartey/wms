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
        #region Methods

        void ChangeFooter<T, I>()
            where T : BindableBase, I
            where I : IViewModel;

        #endregion
    }
}
