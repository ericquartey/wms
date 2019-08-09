using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.Models
{
    public class NavigationMenuItem : BindableBase
    {
        #region Fields

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private string description;

        private bool isEnabled;

        private string moduleName;

        private ICommand navigateCommand;

        private string viewModelName;

        #endregion

        #region Constructors

        public NavigationMenuItem(string viewModelName, string moduleName, string description)
        {
            this.ViewModelName = viewModelName;
            this.ModuleName = moduleName;
            this.Description = description;

            this.IsEnabled = true;
        }

        #endregion

        #region Properties

        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.SetProperty(ref this.isEnabled, value))
                {
                    ((DelegateCommand)this.NavigateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ModuleName
        {
            get => this.moduleName;
            set => this.SetProperty(ref this.moduleName, value);
        }

        public ICommand NavigateCommand => this.navigateCommand ?? (this.navigateCommand = new DelegateCommand(this.Navigate, this.CanNavigate));

        public string ViewModelName
        {
            get => this.viewModelName;
            set => this.SetProperty(ref this.viewModelName, value);
        }

        #endregion

        #region Methods

        private bool CanNavigate()
        {
            return this.IsEnabled;
        }

        private void Navigate()
        {
            this.navigationService.Appear(this.moduleName, this.viewModelName, true);
        }

        #endregion
    }
}
