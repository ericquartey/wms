using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public abstract class CreateViewModel<T> : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
        where T : BusinessObject
    {
        #region Fields

        private readonly ChangeDetector<T> changeDetector = new ChangeDetector<T>();

        private readonly IDialogService dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

        private ICommand clearCommand;

        private ICommand closeDialogCommand;

        private ICommand createCommand;

        private bool isBusy;

        private bool isValidationEnabled;

        private T model;

        #endregion

        #region Constructors

        public CreateViewModel()
        {
            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        #endregion

        #region Properties

        public ICommand ClearCommand => this.clearCommand ??
            (this.clearCommand = new DelegateCommand(
                this.ExecuteClearCommand,
                this.CanExecuteClearCommand));

        public ICommand CloseDialogCommand => this.closeDialogCommand ??
                                           (this.closeDialogCommand = new Prism.Commands.DelegateCommand(
                               this.ExecuteCloseDialogCommand));

        public ICommand CreateCommand => this.createCommand ??
            (this.createCommand = new DelegateCommand(
                async () => await this.ExecuteCreateCommand(),
                this.CanExecuteCreateCommand));

        public IDialogService DialogService => this.dialogService;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        public bool IsValidationEnabled
        {
            get => this.isValidationEnabled;
            set
            {
                if (this.SetProperty(ref this.isValidationEnabled, value))
                {
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        public T Model
        {
            get => this.model;
            set
            {
                if (this.model != null)
                {
                    this.model.PropertyChanged -= this.Model_PropertyChanged;
                }

                if (this.SetProperty(ref this.model, value))
                {
                    this.changeDetector.TakeSnapshot(this.model);

                    if (this.model != null)
                    {
                        this.model.PropertyChanged += this.Model_PropertyChanged;
                    }

                    this.LoadRelatedData();
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        #endregion

        #region Methods

        public override bool CanDisappear()
        {
            if (this.changeDetector.IsModified)
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

        public virtual void LoadRelatedData()
        {
            // do nothing. The derived classes can customize the behaviour
        }

        protected virtual bool CanExecuteClearCommand()
        {
            return this.changeDetector.IsModified == true
                && this.IsBusy == false;
        }

        protected virtual bool CanExecuteCreateCommand()
        {
            return this.Model != null
                && this.changeDetector.IsModified
                && (!this.isValidationEnabled || string.IsNullOrWhiteSpace(this.Model.Error))
                && !this.IsBusy
                && this.changeDetector.IsRequiredValid;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.ClearCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.CreateCommand)?.RaiseCanExecuteChanged();
        }

        protected abstract void ExecuteClearCommand();

        protected void ExecuteCloseDialogCommand()
        {
            this.Disappear();
        }

        protected abstract Task ExecuteCreateCommand();

        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (this.model != null)
            {
                this.model.PropertyChanged -= this.Model_PropertyChanged;
            }
        }

        protected void TakeModelSnapshot()
        {
            this.changeDetector.TakeSnapshot(this.model);
        }

        private void ChangeDetector_ModifiedChanged(object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        #endregion
    }
}
