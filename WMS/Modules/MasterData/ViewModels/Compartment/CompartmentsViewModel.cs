using System;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentsViewModel : EntityListViewModel<Compartment>
    {
        #region Fields

        private ICommand showDetailsCommand;

        #endregion Fields

        #region Properties

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                          (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
            .ObservesProperty(() => this.CurrentItem));

        #endregion Properties

        #region Methods

        private Boolean CanShowDetailsCommand()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        #endregion Methods
    }
}
