using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class DrawerWaitViewModel : BindableBase, IDrawerWaitViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public DrawerWaitViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion

    }
}
