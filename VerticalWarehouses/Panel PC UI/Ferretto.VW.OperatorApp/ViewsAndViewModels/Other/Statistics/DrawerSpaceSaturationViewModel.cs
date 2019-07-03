namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Ferretto.VW.OperatorApp.Interfaces;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Unity;

    public class DrawerSpaceSaturationViewModel : BindableBase, IDrawerSpaceSaturationViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private ICommand drawerWeightSaturationButtonCommand;

        private IUnityContainer container;

        #endregion

        #region Constructors

        public DrawerSpaceSaturationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DrawerWeightSaturationButtonCommand => this.drawerWeightSaturationButtonCommand ?? (this.drawerWeightSaturationButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<DrawerWeightSaturationViewModel, IDrawerWeightSaturationViewModel>();
        }));


        public BindableBase NavigationViewModel { get; set; }

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
