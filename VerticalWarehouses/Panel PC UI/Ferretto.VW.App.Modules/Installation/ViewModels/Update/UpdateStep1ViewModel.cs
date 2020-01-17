using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class UpdateStep1ViewModel : BaseUpdateViewModel
    {
        #region Constructors

        public UpdateStep1ViewModel()
            : base()
        {
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private void NavigationCheck()
        {
            try
            {
                this.IsBusy = true;

                if (this.Installations.Count == 0)
                {
                    this.ShowNotification(InstallationApp.FileReadError, Services.Models.NotificationSeverity.Error);
                    this.IsBusy = false;
                    return;
                }

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Update.STEP2,
                    this.Installations,
                    trackCurrentView: false);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        #endregion
    }
}
