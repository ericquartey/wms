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
using System.Collections.Generic;
using System.ComponentModel;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class AlarmViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private List<MachineError> machineErrors;

        #endregion

        #region Constructors

        public AlarmViewModel(
            IMachineErrorsWebService machineErrorsWebService)
                    : base()
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
        }

        #endregion

        #region Properties

        public List<MachineError> MachineErrors
        {
            get => this.machineErrors;
            set => this.SetProperty(ref this.machineErrors, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                var lst = await this.machineErrorsWebService.GetAllAsync();
                this.MachineErrors = lst.OrderByDescending(o => o.OccurrenceDate).ToList();
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
