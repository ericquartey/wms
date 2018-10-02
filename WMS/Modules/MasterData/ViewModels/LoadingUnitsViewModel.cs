using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : FilteredNavigationViewModel<LoadingUnit>
    {
        #region Fields

        private ICommand viewDetailsCommand;

        #endregion Fields

        #region Properties

        public ICommand ViewDetailsCommand => this.viewDetailsCommand ??
            (this.viewDetailsCommand = new DelegateCommand(this.ExecuteViewDetailsCommand));

        #endregion Properties

        #region Methods

        private void ExecuteViewDetailsCommand()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ShowDetailsEventArgs<LoadingUnit>(this.Token, true));
        }

        #endregion Methods
    }
}
