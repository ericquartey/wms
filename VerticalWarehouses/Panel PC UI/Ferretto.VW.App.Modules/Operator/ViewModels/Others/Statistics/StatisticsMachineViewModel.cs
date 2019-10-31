using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class StatisticsMachineViewModel : BaseMainViewModel
    {
        #region Fields

        private MachineStatistics model;

        #endregion

        #region Constructors

        public StatisticsMachineViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

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
