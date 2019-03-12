using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace Ferretto.VW.OperatorApp.Resources
{
    public class OperatorAppModule : IModule
    {
        #region Fields

        private IUnityContainer container;

        #endregion

        #region Constructors

        public OperatorAppModule(IUnityContainer container)
        {
            this.container = container;

            var mainWindowVMInstance = new MainWindowViewModel();
            var mainWindowInstance = new MainWindow();

            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        #endregion
    }
}
