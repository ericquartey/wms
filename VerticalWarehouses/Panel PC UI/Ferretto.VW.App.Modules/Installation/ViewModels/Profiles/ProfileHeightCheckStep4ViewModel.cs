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
    internal sealed class ProfileHeightCheckStep4ViewModel : BaseProfileHeightCheckViewModel
    {
        #region Constructors

        public ProfileHeightCheckStep4ViewModel(
            IEventAggregator eventAggregator,
            IMachineProfileProcedureWebService profileProcedureService,
            IMachineModeService machineModeService,
            IBayManager bayManager)
            : base(eventAggregator, profileProcedureService, machineModeService, bayManager)
        {
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.currentStep = ProfileHeightCheckStep.ShapePosition;
        }

        protected override void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP3);
            this.ShowNextStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.ProfileHeightCheck.STEP5);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
