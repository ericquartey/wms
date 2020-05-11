using System;
using System.ComponentModel;
using System.Windows;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Fields

        private ConfigurationService configurationService;

        private IOperationResult currentMode;

        private IOperationResult exportConfigurationViewModel;

        private IOperationResult invertersViewModel;

        private IOperationResult setParametersViewModel;

        private IOperationResult vertimagConfigurationViewModel;

        #endregion

        #region Properties

        public IOperationResult CurrentMode
        {
            get => this.currentMode;
            set => this.SetProperty(ref this.currentMode, value);
        }

        #endregion

        #region Methods

        public void StartInstallation()
        {
            this.configurationService = ConfigurationService.GetInstance;
            this.configurationService.PropertyChanged += this.InstallationService_PropertyChanged;
            this.configurationService.Start();
        }

        private void Close()
        {
            Application.Current.Shutdown(this.CurrentMode?.IsSuccessful == true ? 0 : -1);
        }

        private void InstallationService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WizardMode))
            {
                switch (this.configurationService.WizardMode)
                {
                    case WizardMode.None:
                        throw new InvalidOperationException("Can't change on current mode:Wizard not supported.");

                    case WizardMode.ImportConfiguration:
                        this.CurrentMode = this.vertimagConfigurationViewModel ?? (this.vertimagConfigurationViewModel = new VertimagConfigurationViewModel(this.configurationService));
                        break;

                    case WizardMode.Inverters:
                        this.CurrentMode = this.invertersViewModel ?? (this.invertersViewModel = new InvertersViewModel(this.configurationService));
                        break;

                    case WizardMode.Parameters:
                        this.CurrentMode = this.setParametersViewModel ?? (this.setParametersViewModel = new SetParametersViewModel(this.configurationService));
                        break;

                    case WizardMode.ExportConfiguration:
                        this.CurrentMode = this.exportConfigurationViewModel ?? (this.exportConfigurationViewModel = new ExportConfigurationViewModel(this.configurationService));
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
