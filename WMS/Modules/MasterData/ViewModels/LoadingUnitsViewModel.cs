using System;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : EntityListViewModel<LoadingUnit>
    {
        private ICommand withdrawCommand;

        public ICommand WithdrawCommand => this.withdrawCommand ??
        (this.withdrawCommand = new DelegateCommand(this.ExecuteWithdrawCommand));

        private void ExecuteWithdrawCommand()
        {
            throw new NotImplementedException();
        }
    }
}
