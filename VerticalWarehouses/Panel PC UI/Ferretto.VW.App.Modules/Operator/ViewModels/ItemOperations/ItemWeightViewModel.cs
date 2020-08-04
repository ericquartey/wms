using System;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels.ItemOperations
{
    public class ItemWeightViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly DelegateCommand updateAverageWeightCommand;

        #endregion

        #region Constructors

        public ItemWeightViewModel(PresentationMode mode)
            : base(mode)
        {
            this.updateAverageWeightCommand = new DelegateCommand(this.UpdateAverageWeight);
        }

        #endregion

        #region Properties

        public ICommand UpdateAverageWeightCommand => this.updateAverageWeightCommand;

        #endregion

        #region Methods

        private void UpdateAverageWeight()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.WEIGHT_UPDATE);
        }

        #endregion
    }
}
