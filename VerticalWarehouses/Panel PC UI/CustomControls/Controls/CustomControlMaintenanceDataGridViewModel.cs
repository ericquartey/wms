using System;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomControlMaintenanceDataGridViewModel : BindableBase, ICustomControlMaintenanceDataGridViewModel
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
