using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels
{
    public class IdleViewModel : BaseViewModel, IIdleViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public IdleViewModel(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion
    }
}
