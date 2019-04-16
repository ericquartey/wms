using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public abstract class DetailsViewModel<TModel> : BaseServiceNavigationViewModel, IExtensionDataEntityViewModel
        where TModel : class, ICloneable, IModel<int>, INotifyPropertyChanged, IDataErrorInfo, IPolicyDescriptor<IPolicy>
    {
        #region Fields

        private readonly ChangeDetector<TModel> changeDetector = new ChangeDetector<TModel>();

        private string addReason;

        private ColorRequired colorRequired = ColorRequired.EditMode;

        private ICommand deleteCommand;

        private string deleteReason;

        private bool isBusy;

        private bool isModelValid;

        private TModel model;

        private object modelChangedEventSubscription;

        private ICommand refreshCommand;

        private ICommand revertCommand;

        private ICommand saveCommand;

        private string saveReason;

        #endregion

        #region Constructors

        protected DetailsViewModel()
        {
            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;

            this.SubscribeToEvents();
        }

        #endregion

        #region Properties

        public string AddReason
        {
            get => this.addReason;
            set => this.SetProperty(ref this.addReason, value);
        }

        public ColorRequired ColorRequired
        {
            get => this.colorRequired;
            set => this.SetProperty(ref this.colorRequired, value);
        }

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
            async () => await this.ExecuteDeleteWithPromptAsync()));

        public string DeleteReason
        {
            get => this.deleteReason;
            set => this.SetProperty(ref this.deleteReason, value);
        }

        public IDialogService DialogService { get; } = ServiceLocator.Current.GetInstance<IDialogService>();

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

        public bool IsModelIdValid => this.Model?.Id > 0;

        public bool IsModelValid
        {
            get
            {
                bool temp;
                if (!this.changeDetector.IsModified || this.Model == null)
                {
                    temp = true;
                }
                else
                {
                    temp = string.IsNullOrWhiteSpace(this.Model.Error);
                }

                this.SetProperty(ref this.isModelValid, temp);
                return temp;
            }
        }

        public TModel Model
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

                    this.UpdateReasons();
                    this.LoadRelatedData();
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        public ICommand RefreshCommand => this.refreshCommand ??
                    (this.refreshCommand = new DelegateCommand(
                async () => await this.ExecuteRefreshCommandAsync(), this.CanExecuteRefreshCommand));

        public ICommand RevertCommand => this.revertCommand ??
            (this.revertCommand = new DelegateCommand(
                async () => await this.ExecuteRevertWithPromptAsync(),
                this.CanExecuteRevertCommand));

        public ICommand SaveCommand => this.saveCommand ??
            (this.saveCommand = new WmsCommand(
                async () => await this.ExecuteSaveCommandAsync(),
                () => true,
                () => this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error))));

        public string SaveReason
        {
            get => this.saveReason;
            set => this.SetProperty(ref this.saveReason, value);
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

        public void ShowErrorDialog(string message)
        {
            this.DialogService.ShowMessage(
                message,
                DesktopApp.ConfirmOperation,
                DialogType.Warning,
                DialogButtons.OK);
        }

        public virtual void UpdateReasons()
        {
            this.AddReason = this.Model?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Create)).Select(p => p.Reason).FirstOrDefault();
            this.DeleteReason = this.Model?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Delete)).Select(p => p.Reason).FirstOrDefault();
            this.SaveReason = this.Model?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Update)).Select(p => p.Reason).FirstOrDefault();
        }

        protected virtual bool CanExecuteRevertCommand()
        {
            return this.changeDetector.IsModified == true
                && this.IsBusy == false;
        }

        protected virtual bool CanExecuteSaveCommand()
        {
            return this.Model != null
                && this.changeDetector.IsModified
                && this.IsModelValid
                && !this.IsBusy
                && this.changeDetector.IsRequiredValid;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.RefreshCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Performs the action associated to the entity deletion.
        /// </summary>
        /// <returns>True if the action was successful, False otherwise.</returns>
        protected virtual Task<bool> ExecuteDeleteCommandAsync()
        {
            // do nothing: derived classes can customize the behaviour of this command
            return Task.FromResult(false);
        }

        protected abstract Task ExecuteRefreshCommandAsync();

        protected abstract Task ExecuteRevertCommandAsync();

        protected virtual Task<bool> ExecuteSaveCommandAsync()
        {
            // TODO: will be rewritten in scope of Task
            // https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/2158
            dynamic dynamicModel = this.Model;

            if (!PolicyExtensions.CanUpdate(dynamicModel))
            {
                this.ShowErrorDialog(PolicyExtensions.GetCanUpdateReason(dynamicModel));

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        protected abstract Task LoadDataAsync();

        protected virtual void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateReasons();
            this.EvaluateCanExecuteCommands();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelChangedPubSubEvent>(this.modelChangedEventSubscription);
            if (this.model != null)
            {
                this.model.PropertyChanged -= this.Model_PropertyChanged;
            }

            base.OnDispose();
        }

        protected void TakeModelSnapshot()
        {
            this.changeDetector.TakeSnapshot(this.model);
        }

        private bool CanExecuteRefreshCommand()
        {
            return !this.changeDetector.IsModified
                && !this.IsBusy;
        }

        private void ChangeDetector_ModifiedChanged(object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        private async Task ExecuteDeleteWithPromptAsync()
        {
            if (!this.model.CanDelete())
            {
                this.ShowErrorDialog(this.model.GetCanDeleteReason());
                return;
            }

            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, string.Empty),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                var success = await this.ExecuteDeleteCommandAsync();
                if (success)
                {
                    this.HistoryViewService.Previous();
                }
            }
        }

        private async Task ExecuteRevertWithPromptAsync()
        {
            var result = this.DialogService.ShowMessage(
                DesktopApp.AreYouSureToRevertChanges,
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                await this.ExecuteRevertCommandAsync();
            }
        }

        private void SubscribeToEvents()
        {
            var attribute = typeof(TModel)
              .GetCustomAttributes(typeof(ResourceAttribute), true)
              .FirstOrDefault() as ResourceAttribute;

            if (attribute != null)
            {
                this.modelChangedEventSubscription = this.EventService
                    .Subscribe<ModelChangedPubSubEvent>(
                    async eventArgs =>
                    {
                        await this.LoadDataAsync().ConfigureAwait(true);
                    },
                    false,
                    e => e.ResourceName == attribute.ResourceName
                        &&
                        this.model != null
                        &&
                        (int)e.ResourceId == this.model.Id);
            }
        }

        #endregion
    }
}
