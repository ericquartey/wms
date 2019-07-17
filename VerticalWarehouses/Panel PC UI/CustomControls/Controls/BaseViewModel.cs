using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class BaseViewModel : BindableBase, IViewModel
    {
        public BindableBase NavigationViewModel { get; set; }

        virtual public void ExitFromViewMethod()
        {
            // do nothing
        }

        virtual public Task OnEnterViewAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        virtual public void UnSubscribeMethodFromEvent()
        {
            // do nothing
        }
    }
}
