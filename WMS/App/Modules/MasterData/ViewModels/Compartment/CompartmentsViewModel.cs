using System.Windows.Input;
using Ferretto.Common.Controls;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentsViewModel : EntityPagedListViewModel<Compartment, int>
    {
        #region Fields

        private ICommand showCompartmentDetailsCommand;

        #endregion

        #region Properties

        public ICommand ShowCompartmentDetailsCommand => this.showCompartmentDetailsCommand ??
                          (this.showCompartmentDetailsCommand = new DelegateCommand(this.ShowCompartmentDetails, this.CanShowCompartmentDetails)
            .ObservesProperty(() => this.CurrentItem));

        #endregion

        #region Methods

        private bool CanShowCompartmentDetails()
        {
            return this.CurrentItem != null;
        }

        private void ShowCompartmentDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.COMPARTMENTDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
