using System;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class MainWindowBackToIAPPButtonViewModel : BindableBase, IMainWindowBackToIAPPButtonViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private IUnityContainer container;
        private bool isBackButtonActive = true;
        private bool isCancelButtonActive;

        #endregion Fields

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        public CompositeCommand CancelButtonCommand { get; set; }

        public Boolean IsBackButtonActive { get => this.isBackButtonActive; set => this.SetProperty(ref this.isBackButtonActive, value); }

        public Boolean IsCancelButtonActive { get => this.isCancelButtonActive; set => this.SetProperty(ref this.isCancelButtonActive, value); }

        #endregion Properties

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

        #endregion Methods
    }
}
