﻿using System.Threading.Tasks;
using System.Windows;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowBackToIAPPButtonViewModel : BindableBase, IMainWindowBackToIAPPButtonViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private Visibility cancelButtonVisibility = Visibility.Hidden;

        private IUnityContainer container;

        private bool isBackButtonActive = true;

        private bool isCancelButtonActive;

        #endregion

        #region Constructors

        public MainWindowBackToIAPPButtonViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        public CompositeCommand CancelButtonCommand { get; set; }

        public Visibility CancelButtonVisibility { get => this.cancelButtonVisibility; set => this.SetProperty(ref this.cancelButtonVisibility, value); }

        public bool IsBackButtonActive { get => this.isBackButtonActive; set => this.SetProperty(ref this.isBackButtonActive, value); }

        public bool IsCancelButtonActive { get => this.isCancelButtonActive; set => this.SetProperty(ref this.isCancelButtonActive, value); }

        public BindableBase NavigationViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void FinalizeBottomButtons()
        {
            this.BackButtonCommand = null;
        }

        public void InitializeBottomButtons()
        {
            this.BackButtonCommand = new CompositeCommand();
            this.BackButtonCommand.RegisterCommand(((MainWindowViewModel)this.container.Resolve<IMainWindowViewModel>()).BackToMainWindowNavigationButtonsViewButtonCommand);
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public Task OnEnterViewAsync()
        {
            return null;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
