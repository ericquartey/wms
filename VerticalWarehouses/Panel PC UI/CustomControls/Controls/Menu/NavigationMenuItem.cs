﻿using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls
{
    public class NavigationMenuItem : BindableBase, System.IDisposable
    {
        #region Fields

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private readonly object subscriptionToken;

        private string description;

        private bool disposedValue = false;

        private bool isActive;

        private bool isEnabled;

        private string moduleName;

        private ICommand navigateCommand;

        private string viewModelName;

        #endregion

        #region Constructors

        public NavigationMenuItem(string viewModelName, string moduleName, string description, bool trackCurrentView)
        {
            this.ViewModelName = viewModelName;
            this.ModuleName = moduleName;
            this.Description = description;
            this.TrackCurrentView = trackCurrentView;
            this.IsEnabled = true;

            this.subscriptionToken = this.navigationService.SubscribeToNavigationCompleted(
                e => this.IsActive =
                this.ViewModelName == e.ViewModelName
                &&
                this.ModuleName == e.ModuleName);
        }

        #endregion

        #region Properties

        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        public bool IsActive
        {
            get => this.isActive;
            set => this.SetProperty(ref this.isActive, value);
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

        public ICommand NavigateCommand =>
            this.navigateCommand
            ??
            (this.navigateCommand = new DelegateCommand(() => this.Navigate(), this.CanNavigate));

        public bool TrackCurrentView { get; }

        public string ViewModelName
        {
            get => this.viewModelName;
            set => this.SetProperty(ref this.viewModelName, value);
        }

        #endregion

        #region Methods

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.navigationService.UnsubscribeToNavigationCompleted(this.subscriptionToken);
                }

                this.disposedValue = true;
            }
        }

        private bool CanNavigate()
        {
            return this.IsEnabled;
        }

        private void Navigate()
        {
            this.navigationService.Appear(this.moduleName, this.viewModelName, null, this.TrackCurrentView);
        }

        #endregion
    }
}
