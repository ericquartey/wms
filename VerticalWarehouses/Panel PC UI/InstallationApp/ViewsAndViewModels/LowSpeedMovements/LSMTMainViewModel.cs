using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.LowSpeedMovements
{
    public class LSMTMainViewModel : BindableBase, ILSMTMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private BindableBase lSMTContentRegionCurrentViewModel;

        private ILSMTNavigationButtonsViewModel lSMTNavigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public LSMTMainViewModel(
            IEventAggregator eventAggregator,
            ILSMTNavigationButtonsViewModel navigationViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.LSMTNavigationRegionCurrentViewModel = navigationViewModel;

            this.LSMTContentRegionCurrentViewModel = null;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase LSMTContentRegionCurrentViewModel { get => this.lSMTContentRegionCurrentViewModel; set => this.SetProperty(ref this.lSMTContentRegionCurrentViewModel, value); }

        public ILSMTNavigationButtonsViewModel LSMTNavigationRegionCurrentViewModel
        {
            get => this.lSMTNavigationRegionCurrentViewModel;
            set => this.SetProperty(ref this.lSMTNavigationRegionCurrentViewModel, value);
        }

        public BindableBase NavigationViewModel { get; set; }

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
