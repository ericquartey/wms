using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerCompactingViewModel : BaseMainViewModel
    {
        #region Fields

        private ICommand drawerCompactingDetailButtonCommand;

        private bool isWaitingForResponse;

        #endregion

        #region Constructors

        public DrawerCompactingViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Properties

        public ICommand DrawerCompactingDetailButtonCommand => this.drawerCompactingDetailButtonCommand ?? (this.drawerCompactingDetailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        public override EnableMask EnableMask => EnableMask.None;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

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
