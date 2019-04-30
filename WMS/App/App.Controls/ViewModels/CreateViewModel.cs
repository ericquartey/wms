using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public abstract class CreateViewModel<TModel> : BaseDialogViewModel<TModel>, IExtensionDataEntityViewModel
        where TModel : class, ICloneable, IModel<int>, INotifyPropertyChanged, IDataErrorInfo
    {
        #region Fields

        private readonly IDialogService dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

        private ICommand clearCommand;

        private ICommand createCommand;

        #endregion

        #region Properties

        public ICommand ClearCommand => this.clearCommand ??
            (this.clearCommand = new DelegateCommand(
                async () => await this.ExecuteClearCommandAsync(),
                this.CanExecuteClearCommand));

        public ColorRequired ColorRequired => ColorRequired.CreateMode;

        public ICommand CreateCommand => this.createCommand ??
            (this.createCommand = new WmsCommand(
                async () => await this.ExecuteCreateCommandAsync(),
                this.CanExecuteCreateCommand,
                () => this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error))));

        public IDialogService DialogService => this.dialogService;

        #endregion

        #region Methods

        public override bool CanDisappear()
        {
            if (this.ChangeDetector.IsModified)
            {
                var result = this.DialogService.ShowMessage(
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

        protected virtual bool CanExecuteClearCommand()
        {
            return this.ChangeDetector.IsModified
                && this.IsBusy == false;
        }

        protected virtual bool CanExecuteCreateCommand()
        {
            var canExecute = this.Model != null
                && this.ChangeDetector.IsModified
                && this.IsModelValid
                && !this.IsBusy
                && this.ChangeDetector.IsRequiredValid;

            if (canExecute)
            {
                this.CanShowError = true;
            }

            return canExecute;
        }

        protected override void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.ClearCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.CreateCommand)?.RaiseCanExecuteChanged();
        }

        protected abstract Task ExecuteClearCommandAsync();

        protected abstract Task ExecuteCreateCommandAsync();

        #endregion
    }
}
