using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Ferretto.VW.InstallationApp
{
    public interface IViewModelRequiresContainer
    {
        #region Methods

        void InitializeViewModel(IUnityContainer container);

        #endregion
    }
}
