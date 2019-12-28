using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseOperatorViewModel : BaseMainViewModel, IRegionMemberLifetime
    {
        #region Fields

        private SubscriptionToken barcodeMatchedToken;

        #endregion

        #region Constructors

        protected BaseOperatorViewModel(PresentationMode mode)
            : base(mode)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public override bool KeepAlive => true;

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.barcodeMatchedToken = this.barcodeMatchedToken
                ??
                this.EventAggregator
                    .GetEvent<PubSubEvent<BarcodeMatchEventArgs>>()
                    .Subscribe(
                        async e => await this.OnBarcodeMatchedAsync(e),
                        ThreadOption.UIThread,
                        false);
        }

        protected virtual Task OnBarcodeMatchedAsync(BarcodeMatchEventArgs e)
        {
            // do nothing
            return Task.CompletedTask;
        }

        #endregion
    }
}
