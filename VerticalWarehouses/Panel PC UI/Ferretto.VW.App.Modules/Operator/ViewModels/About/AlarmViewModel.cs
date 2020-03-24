using System.Threading.Tasks;
using System;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class AlarmViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineErrorsWebService machineErrorsWebService;

        #endregion

        #region Constructors

        public AlarmViewModel(IMachineErrorsWebService machineErrorsWebService)
                    : base()
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            // TODO: Insert code here

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                var list = await this.machineErrorsWebService.GetAllAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
