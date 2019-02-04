using System.Windows.Input;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
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

        #endregion

        #region Properties

        public ICommand AcceptCommand => this.acceptCommand ??
            (this.acceptCommand = new DelegateCommand(this.ExecuteAcceptCommand));

        public CriteriaOperator Filter
        {
            get => this.filter;
            set => this.SetProperty(ref this.filter, value);
        }

        public FilteringUIContext FilteringContext
        {
            get => this.filteringContext;
            set => this.SetProperty(ref this.filteringContext, value);
        }

        public ICommand RevertCommand => this.revertCommand ??
            (this.revertCommand = new DelegateCommand(this.ExecuteRevertCommand));

        #endregion

        #region Methods

        protected override void OnAppear()
        {
            if (this.Data is FilterDialogData filterDialogData)
            {
                this.FilteringContext = filterDialogData.FilteringContext;

                if (filterDialogData.Filter is null == false)
                {
                    this.Filter = CriteriaOperator.Parse(filterDialogData.Filter.ToString());
                }
            }
        }

        private void ExecuteAcceptCommand()
        {
            this.EventService.Invoke(new FilteringChangedPubSubEvent(this.filter, this.filteringContext));

            this.Disappear();
        }

        private void ExecuteRevertCommand()
        {
            this.Disappear();
        }

        #endregion
    }
}
