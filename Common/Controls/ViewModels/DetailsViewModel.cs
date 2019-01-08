using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public abstract class DetailsViewModel<T> : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
        where T : BusinessObject
    {
        #region Fields

        private readonly ChangeDetector<T> changeDetector = new ChangeDetector<T>();

        private readonly IDialogService dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

        private bool isBusy;

        private bool isValidationEnabled;

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

                    this.RefreshData();
                }
            }
        }

        public ICommand RevertCommand => this.revertCommand ??
            (this.revertCommand = new DelegateCommand(
                async () => await this.ExecuteRevertWithPrompt(),
                this.CanExecuteRevertCommand));

        public ICommand SaveCommand => this.saveCommand ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.ExecuteSaveCommand(),
                this.CanExecuteSaveCommand));

        #endregion Properties

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

        public virtual void RefreshData()
        {
            this.EvaluateCanExecuteCommands();
        }

        protected virtual bool CanExecuteRevertCommand()
        {
            return this.changeDetector.IsModified == true
                && this.IsBusy == false;
        }

        protected virtual bool CanExecuteSaveCommand()
        {
            return this.Model != null
                && this.changeDetector.IsModified == true
                && (this.isValidationEnabled == false || string.IsNullOrWhiteSpace(this.Model.Error))
                && this.IsBusy == false;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        protected abstract Task ExecuteRevertCommand();

        protected abstract Task ExecuteSaveCommand();

        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            this.model.PropertyChanged -= this.Model_PropertyChanged;
        }

        protected void TakeModelSnapshot()
        {
            this.changeDetector.TakeSnapshot(this.model);
        }

        private void ChangeDetector_ModifiedChanged(object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        private async Task ExecuteRevertWithPrompt()
        {
            var result = this.DialogService.ShowMessage(
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
