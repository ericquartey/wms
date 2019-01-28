using System;
using System.Windows.Input;
using DevExpress.Xpf.Core.FilteringUI;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class FilterDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private ICommand applyFilterCommand;

        private ICommand clearFilterCommand;

        private ICommand closeCommand;

        private FilteringUIContext filterContext;

        private bool isBusy;

        #endregion Fields

        #region Properties

        public ICommand ApplyFilterCommand => this.applyFilterCommand ??
                                                   (this.applyFilterCommand = new DelegateCommand(
                                                       this.ExecuteApplyFilterCommand));

        public ICommand ClearFilterCommand => this.clearFilterCommand ??
                                                 (this.clearFilterCommand = new DelegateCommand(
                                                     this.ExecuteClearFilterCommand));

        public ICommand CloseCommand => this.closeCommand ??
                                                 (this.closeCommand = new DelegateCommand(
                                                     this.ExecuteCloseCommand));

        public FilteringUIContext FilterContext
        {
            get => this.filterContext;
            set => this.SetProperty(ref this.filterContext, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            if (this.Data is FilteringUIContext filterContext)
            {
                this.FilterContext = filterContext;
            }
        }

        private void ExecuteApplyFilterCommand()
        {
            throw new NotImplementedException();
        }

        private void ExecuteClearFilterCommand()
        {
            throw new NotImplementedException();
        }

        private void ExecuteCloseCommand()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
