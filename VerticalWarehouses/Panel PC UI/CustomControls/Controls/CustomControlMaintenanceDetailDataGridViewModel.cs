using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlMaintenanceDetailDataGridViewModel : BindableBase, ICustomControlMaintenanceDetailDataGridViewModel
    {
        #region Properties

        public BindableBase NavigationViewModel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new NotImplementedException();
        }

        public Task OnEnterViewAsync()
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
