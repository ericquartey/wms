using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class DevicesViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly List<DeviceBase> devices;

        private readonly IMachineDevicesService machineDevicesServices = ServiceLocator.Current.GetInstance<IMachineDevicesService>();

        private bool isBusy;

        private ICommand refreshCommand;

        #endregion

        #region Constructors

        public DevicesViewModel() : base(Services.PresentationMode.Installer)
        {
            this.devices = new List<DeviceBase>();
        }

        #endregion

        #region Properties

        public IEnumerable<DeviceBase> Devices => new List<DeviceBase>(this.devices);

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ICommand RefreshCommand =>
                this.refreshCommand
                ??
                (this.refreshCommand = new DelegateCommand(
                    async () => await this.GetDataAsync(),
                    this.CanRefresh));

        #endregion

        #region Methods

        public async Task GetDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.devices.Clear();
                var result = await this.machineDevicesServices.GetAllAsync();
                this.devices.AddRange(result.Item1);
                this.devices.AddRange(result.Item2);
                this.RaisePropertyChanged(nameof(this.Devices));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
                ((DelegateCommand)this.refreshCommand).RaiseCanExecuteChanged();
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            await this.GetDataAsync();
        }

        private bool CanRefresh()
        {
            return !this.isBusy;
        }

        #endregion
    }
}
