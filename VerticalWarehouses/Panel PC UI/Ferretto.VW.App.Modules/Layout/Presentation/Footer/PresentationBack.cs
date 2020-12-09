using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationBack : BasePresentationViewModel
    {
        #region Fields

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public PresentationBack(
            INavigationService navigationService)
            : base(PresentationTypes.Back)
        {
            if (navigationService is null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.navigationService = navigationService;

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            if (this.navigationService.GetActiveViewModel().ToString().Split('.').Last() == Utils.Modules.Operator.ItemSearch.MAIN)
            {
                //this.navigationService.GoBack();

                NavigationItemSearch.OnItemSearchMainView = true;

                this.navigationService.GoBackTo(
                                nameof(Utils.Modules.Operator),
                                Utils.Modules.Operator.ItemOperations.WAIT);
            }
            else if (this.navigationService.GetActiveViewModel().ToString().Split('.').Last() == Utils.Modules.Installation.SHUTTERENDURANCETEST &&
                (this.MachineModeService.MachineMode == MAS.AutomationService.Contracts.MachineMode.Test ||
                this.MachineModeService.MachineMode == MAS.AutomationService.Contracts.MachineMode.Test2 ||
                this.MachineModeService.MachineMode == MAS.AutomationService.Contracts.MachineMode.Test3))
            {
                this.ShowNotification(Resources.Localized.Get("General.BackShutterTest"), Services.Models.NotificationSeverity.Info);
            }
            else
            {
                if (this.navigationService.GetActiveView() is INavigableView view
                &&
                !string.IsNullOrEmpty(view.ParentModuleName)
                &&
                !string.IsNullOrEmpty(view.ParentViewName))
                {
                    //NavigationItemSearch.OnItemSearchMainView = false;
                    this.navigationService.Appear(view.ParentModuleName, view.ParentViewName, null, false);
                }
                else
                {
                    //NavigationItemSearch.OnItemSearchMainView = false;
                    this.navigationService.GoBack();
                }
            }

            return Task.CompletedTask;
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.States != null
                &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation back
                &&
                back.IsVisible.HasValue)
            {
                this.IsVisible = back.IsVisible;
            }

            if (message.States != null
               &&
               message.States.FirstOrDefault(s => s.Type == PresentationTypes.Abort) is Services.Presentation abort
               &&
               abort.IsVisible.HasValue
               &&
               abort.IsVisible.Value)
            {
                this.IsVisible = false;
            }
        }

        #endregion
    }
}
