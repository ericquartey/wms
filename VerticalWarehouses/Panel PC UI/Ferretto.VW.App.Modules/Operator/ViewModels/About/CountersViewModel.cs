using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class CountersViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private MachineStatistics model;

        #endregion

        #region Constructors

        public CountersViewModel(IMachineIdentityWebService machineIdentityWebService)
            : base()
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
        }

        #endregion

        #region Properties

        public MachineStatistics Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            this.Model = await this.machineIdentityWebService.GetStatisticsAsync();

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
        }

        #endregion
    }
}
