using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class OtherMainViewModel : BindableBase, IOtherMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private BindableBase contentRegionCurrentViewModel;

        private BindableBase footerRegionCurrentViewModel;

        private BindableBase navigationRegionCurrentViewModel;

        #endregion

        #region Constructors

        public OtherMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }

        public BindableBase FooterRegionCurrentViewModel { get => this.footerRegionCurrentViewModel; set => this.SetProperty(ref this.footerRegionCurrentViewModel, value); }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public void SubscribeMethodToEvent()
        {
            this.ContentRegionCurrentViewModel = this.container.Resolve<IIdleViewModel>() as IdleViewModel;
            this.NavigationRegionCurrentViewModel = this.container.Resolve<IOtherNavigationViewModel>() as OtherNavigationViewModel;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
