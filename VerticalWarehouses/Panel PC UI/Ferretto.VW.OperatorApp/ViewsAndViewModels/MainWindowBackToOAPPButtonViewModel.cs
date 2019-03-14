using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class MainWindowBackToOAPPButtonViewModel : BindableBase, IMainWindowBackToOAPPButtonViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MainWindowBackToOAPPButtonViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new NotImplementedException();
        }

        public void SubscribeMethodToEvent()
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
