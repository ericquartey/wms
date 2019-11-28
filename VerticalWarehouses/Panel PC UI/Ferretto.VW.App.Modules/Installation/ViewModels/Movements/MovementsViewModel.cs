using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class MovementsViewModel : BaseMainViewModel
    {
        #region Constructors

        public MovementsViewModel()
            : base(PresentationMode.Installer)
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
            await base.OnAppearedAsync();

            try
            {
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
