using System;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowBackToIAPPButtonViewModel : BindableBase, IMainWindowBackToIAPPButtonViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private Visibility cancelButtonVisibility = Visibility.Hidden;

        private IUnityContainer container;

        private bool isBackButtonActive = true;

        private bool isCancelButtonActive = false;

        #endregion

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        public CompositeCommand CancelButtonCommand { get; set; }

        public Visibility CancelButtonVisibility { get => this.cancelButtonVisibility; set => this.SetProperty(ref this.cancelButtonVisibility, value); }

        public bool IsBackButtonActive { get => this.isBackButtonActive; set => this.SetProperty(ref this.isBackButtonActive, value); }

        public bool IsCancelButtonActive { get => this.isCancelButtonActive; set => this.SetProperty(ref this.isCancelButtonActive, value); }

        #endregion

        #region Methods

        public void FinalizeBottomButtons()
        {
            this.BackButtonCommand = null;
        }

        public void InitializeBottomButtons()
        {
            this.BackButtonCommand = new CompositeCommand();
            this.BackButtonCommand.RegisterCommand(((MainWindowViewModel)this.container.Resolve<IMainWindowViewModel>()).BackToMainWindowNavigationButtonsViewButtonCommand);
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
        }

        #endregion
    }
}
