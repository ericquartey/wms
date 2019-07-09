using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class GeneralInfoViewModel : BindableBase, IGeneralInfoViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public GeneralInfoViewModel(
            IEventAggregator eventAggregator,
            IOtherNavigationViewModel otherNavigationViewModel)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.OtherNavigationViewModel = otherNavigationViewModel;
            this.NavigationViewModel = otherNavigationViewModel as OtherNavigationViewModel;
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public IOtherNavigationViewModel OtherNavigationViewModel { get; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
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
