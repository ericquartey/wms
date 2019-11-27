using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Menu.ViewModels
{
    internal sealed class AccessoriesMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isWaitingForResponse;

        #endregion

        #region Constructors

        public AccessoriesMenuViewModel()
            : base(PresentationMode.Menu)
        {
        }

        #endregion

        #region Properties

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
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

        private void RaiseCanExecuteChanged()
        {
        }

        #endregion
    }
}
