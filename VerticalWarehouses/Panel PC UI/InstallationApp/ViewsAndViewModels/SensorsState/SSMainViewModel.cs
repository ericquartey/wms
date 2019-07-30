using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.Utils.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState
{
    public class SSMainViewModel : BindableBase, ISSMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IViewModel sSContentRegionCurrentViewModel;

        private IViewModel sSNavigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public SSMainViewModel(
            IEventAggregator eventAggregator,
            ISSNavigationButtonsViewModel sSNavigationButtonsViewModel)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (sSNavigationButtonsViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(sSNavigationButtonsViewModel));
            }

            this.eventAggregator = eventAggregator;
            this.SSContentRegionCurrentViewModel = null;
            this.SSNavigationRegionCurrentViewModel = sSNavigationButtonsViewModel;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public IViewModel SSContentRegionCurrentViewModel { get => this.sSContentRegionCurrentViewModel; set => this.SetProperty(ref this.sSContentRegionCurrentViewModel, value); }

        public IViewModel SSNavigationRegionCurrentViewModel
        {
            get => this.sSNavigationRegionCurrentViewModel;
            set => this.SetProperty(ref this.sSNavigationRegionCurrentViewModel, value);
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
