using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ProfileHeightCheckStep3ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Constructors

        public ProfileHeightCheckStep3ViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager,
            IMachineService machineService)
            : base(eventAggregator, profileProcedureService, machineModeService, machineService, bayManager)
        {
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = ProfileHeightCheckStep.DrawerPosition;
        }

        protected override void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP2);
            this.ShowNextStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP4);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
