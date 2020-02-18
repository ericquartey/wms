using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    public class StatisticsDrawersViewModel : BaseOperatorViewModel
    {
        #region Fields

        private MachineStatistics model;

        #endregion

        #region Constructors

        public StatisticsDrawersViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public MachineStatistics Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        #endregion
    }
}
