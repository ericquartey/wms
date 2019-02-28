using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Modularity;

namespace Ferretto.VW.VWApp
{
    public class VWAppModule : IModule
    {
        #region Fields

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public VWAppModule(IUnityContainer container)
        {
            this.container = container;
            var eventAggregator = new EventAggregator();
            this.container.RegisterInstance<IEventAggregator>(eventAggregator);
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
