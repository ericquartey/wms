using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class WmsSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private DelegateCommand checkEndpointCommand;

        private IPEndPoint ipEndpoint;

        private bool isWmsEnabled;

        private DelegateCommand saveCommand;

        #endregion

        #region Constructors

        public WmsSettingsViewModel(IMachineWmsStatusWebService wmsStatusWebService)
            : base(PresentationMode.Installer)
        {
            this.wmsStatusWebService = wmsStatusWebService ?? throw new ArgumentNullException(nameof(wmsStatusWebService));
        }

        #endregion

        #region Properties

        public ICommand CheckEndpointCommand =>
            this.checkEndpointCommand
            ??
            (this.checkEndpointCommand = new DelegateCommand(
                async () => await this.CheckEndpointAsync(), this.CanCheckEndpoint));

        public IPEndPoint IPEndPoint
        {
            get => this.ipEndpoint;
            set => this.SetProperty(ref this.ipEndpoint, value);
        }

        public bool IsWmsEnabled
        {
            get => this.isWmsEnabled;
            set => this.SetProperty(ref this.isWmsEnabled, value);
        }

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        private bool CanCheckEndpoint()
        {
            throw new NotImplementedException();
        }

        private bool CanSave()
        {
            return true;
        }

        private Task CheckEndpointAsync()
        {
            throw new NotImplementedException();
        }

        private async Task SaveAsync()
        {
            try
            {
                //  await this.wmsStatusWebService.UpdateAsync(this.IsEnabled, this.IPEndPoint);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
            }
        }

        #endregion
    }
}
