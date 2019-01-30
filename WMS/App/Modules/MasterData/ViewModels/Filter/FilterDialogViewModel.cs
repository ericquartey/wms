using System.Windows.Input;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class FilterDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private ICommand acceptCommand;

        private CriteriaOperator filter;

        private FilteringUIContext filteringContext;

        private ICommand revertCommand;

        #endregion Fields

        #region Constructors

        public FilterDialogViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public ICommand AcceptCommand => this.acceptCommand ??
            (this.acceptCommand = new DelegateCommand(this.ExecuteAcceptCommand));

        public CriteriaOperator Filter
        {
            get => this.filter;
            set
            {
                if (this.SetProperty(ref this.filter, value))
                {
                    this.EventService.Invoke(new FilteringChangedPubSubEvent(this.filter, this.filteringContext));
                }
            }
        }

        public FilteringUIContext FilteringContext
        {
            get => this.filteringContext;
            set => this.SetProperty(ref this.filteringContext, value);
        }

        public ICommand RevertCommand => this.revertCommand ??
            (this.revertCommand = new DelegateCommand(this.ExecuteRevertCommand));

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            if (this.Data is FilteringUIContext filteringContext)
            {
                this.FilteringContext = filteringContext;
            }
        }

        private void ExecuteAcceptCommand()
        {
            this.Disappear();
        }

        private void ExecuteRevertCommand()
        {
            this.Disappear();
        }

        #endregion Methods
    }
}
