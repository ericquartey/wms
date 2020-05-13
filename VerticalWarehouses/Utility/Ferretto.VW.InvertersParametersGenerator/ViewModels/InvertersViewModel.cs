using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Ferretto.VW.MAS.DataModels;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class InvertersViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly bool isSuccessful;

        private IEnumerable<InverterParametersDataInfo> invertersParameters;

        private bool isBusy;

        private DelegateCommand nextCommand;

        private string selectedFileConfigurationName;

        #endregion

        #region Constructors

        public InvertersViewModel(ConfigurationService installationService)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
            this.LoadConfiguration();
        }

        #endregion

        #region Properties

        public IEnumerable<InverterParametersDataInfo> InvertersParameters => this.invertersParameters;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsSuccessful => this.isSuccessful;

        public ICommand NextCommand =>
                   this.nextCommand
               ??
               (this.nextCommand = new DelegateCommand(
                this.Next));

        public string SelectedFileConfigurationName
        {
            get => this.selectedFileConfigurationName;
            set => this.SetProperty(ref this.selectedFileConfigurationName, value);
        }

        #endregion

        #region Methods

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private IEnumerable<InverterParametersDataInfo> GetInvertersParameters(VertimagConfiguration vertimagConfiguration)
        {
            var inverterParametersData = new List<InverterParametersDataInfo>();

            foreach (var axe in vertimagConfiguration.Machine.Elevator.Axes)
            {
                if (!(axe.Inverter is null))
                {
                    inverterParametersData.Add(new InverterParametersDataInfo(axe.Inverter.Type, (byte)axe.Inverter.Index, this.GetShortInverterDescription(axe.Inverter.Type, axe.Inverter.IpAddress, axe.Inverter.TcpPort)));
                }
            }

            foreach (var bay in vertimagConfiguration.Machine.Bays)
            {
                if (!(bay.Inverter is null))
                {
                    inverterParametersData.Add(new InverterParametersDataInfo(bay.Inverter.Type, (byte)bay.Inverter.Index, this.GetShortInverterDescription(bay.Inverter.Type, bay.Inverter.IpAddress, bay.Inverter.TcpPort)));
                }
            }

            if (inverterParametersData.Count == 0)
            {
                throw new Exception("No inverters parameters found.");
            }

            return inverterParametersData;
        }

        private string GetShortInverterDescription(InverterType type, IPAddress ipAddress, int tcpPort)
        {
            var port = (tcpPort == 0) ? string.Empty : tcpPort.ToString();
            var ip = (ipAddress is null) ? string.Empty : ipAddress?.ToString();
            var ipPort = (string.IsNullOrEmpty(ip)) ? string.Empty : $"{ip}:{port}";
            return $"{type.ToString()} {ipPort}";
        }

        private void LoadConfiguration()
        {
            try
            {
                this.IsBusy = true;

                if (!(this.configurationService.VertimagConfiguration is null))
                {
                    this.invertersParameters = this.GetInvertersParameters(this.configurationService.VertimagConfiguration);
                }

                this.RaisePropertyChanged(nameof(this.InvertersParameters));
            }
            catch (Exception ex)
            {
                this.configurationService.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void Next()
        {
            this.configurationService.SetInvertersConfiguration(this.invertersParameters);
            this.configurationService.SetWizard(WizardMode.Parameters);
        }

        private void RaiseCanExecuteChanged()
        {
            this.nextCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
