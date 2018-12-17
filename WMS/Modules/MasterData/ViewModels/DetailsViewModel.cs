using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public abstract class DetailsViewModel<T> : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
        where T : BusinessObject
    {
        #region Fields

        private readonly ChangeDetector<T> changeDetector = new ChangeDetector<T>();

        private T model;
        private ICommand revertCommand;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public DetailsViewModel()
        {
            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        #endregion Constructors

        #region Properties

        public T Model
        {
            get => this.model;
            set
            {
                if (this.SetProperty(ref this.model, value))
                {
                    this.changeDetector.TakeSnapshot(this.model);

                    this.RefreshData();
                }
            }
        }

        public ICommand RevertCommand => this.revertCommand ??
                    (this.revertCommand = new DelegateCommand(async () => await this.ExecuteRevertWithPrompt().ConfigureAwait(true), this.CanExecuteRevertCommand));

        public ICommand SaveCommand => this.saveCommand ??
                                       (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand, this.CanExecuteSaveCommand));

        #endregion Properties

        #region Methods

        public override System.Boolean CanDisappear()
        {
            if (this.changeDetector.IsModified)
            {
                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

                var result = dialogService.ShowMessage(
                    DesktopApp.AreYouSureToLeaveThePage,
                    DesktopApp.ConfirmOperation,
                    DialogType.Exclamation,
                    DialogButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public abstract void RefreshData();

        protected virtual bool CanExecuteRevertCommand()
        {
            return this.changeDetector.IsModified == true;
        }

        protected virtual bool CanExecuteSaveCommand()
        {
            return this.changeDetector.IsModified == true && string.IsNullOrWhiteSpace(this.Model.Error);
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        protected abstract Task ExecuteRevertCommand();

        protected abstract void ExecuteSaveCommand();

        protected void TakeModelSnapshot()
        {
            this.changeDetector.TakeSnapshot(this.model);
        }

        private void ChangeDetector_ModifiedChanged(System.Object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        private async Task ExecuteRevertWithPrompt()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

            var result = dialogService.ShowMessage(
                DesktopApp.AreYouSureToRevertChanges,
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                await this.ExecuteRevertCommand();
            }
        }

        #endregion Methods
    }
}
