using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class DrawerCompactingViewModel : BaseOperatorViewModel
    {
        #region Fields

        private ICommand drawerCompactingDetailButtonCommand;

        #endregion

        #region Constructors

        public DrawerCompactingViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerCompactingDetailButtonCommand => this.drawerCompactingDetailButtonCommand ?? (this.drawerCompactingDetailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.DrawerCompacting.DETAIL,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
