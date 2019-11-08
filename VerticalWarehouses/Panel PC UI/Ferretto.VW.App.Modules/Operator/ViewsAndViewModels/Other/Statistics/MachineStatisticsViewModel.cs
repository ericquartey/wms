using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class MachineStatisticsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private MachineStatistics model;

        #endregion

        #region Constructors

        public MachineStatisticsViewModel(IMachineIdentityWebService identityService)
            : base(Services.PresentationMode.Operator)
        {
            this.identityService = identityService ?? throw new System.ArgumentNullException(nameof(identityService));
        }

        #endregion

        #region Properties

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

            try
            {
                this.Model = await this.identityService.GetStatisticsAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
