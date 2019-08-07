using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Resources;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public abstract class DetailsViewModel<TModel> : BaseServiceNavigationViewModel, IExtensionDataEntityViewModel
        where TModel : class, ICloneable, IModel<int>, INotifyPropertyChanged, IDataErrorInfo,
        IPolicyDescriptor<IPolicy>, IValidationEnable
    {
        #region Fields

        private readonly ChangeDetector<TModel> changeDetector = new ChangeDetector<TModel>();

        private string addReason;

        private ColorRequired colorRequired = ColorRequired.EditMode;

        private ICommand deleteCommand;

        private string deleteReason;

        private bool hasConflictingChanges;

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

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteWithPromptAsync(),
                this.CanExecuteDeleteCommand));

        public IDialogService DialogService { get; } = ServiceLocator.Current.GetInstance<IDialogService>();

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

        public ICommand RefreshCommand => this.refreshCommand ??
                    (this.refreshCommand = new DelegateCommand(
                async () => await this.ExecuteRefreshWithPromptAsync(),
                this.CanExecuteRefreshCommand));

        public ICommand RevertCommand => this.revertCommand ??
            (this.revertCommand = new DelegateCommand(
                async () => await this.ExecuteRevertWithPromptAsync(),
                this.CanExecuteRevertCommand));

        public ICommand SaveCommand => this.saveCommand ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.ExecuteSaveWithPromptAsync(),
                this.CanExecuteSaveCommand));

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

        public string DeleteReason
        {
            get => this.deleteReason;
            set => this.SetProperty(ref this.deleteReason, value);
        }

        public bool HasConflictingChanges
        {
            get => this.hasConflictingChanges;
            set
            {
                if (this.SetProperty(ref this.hasConflictingChanges, value))
                {
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

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
                    this.LoadRelatedDataAsync().ConfigureAwait(true);
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

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
                    DialogButtons.YesNo);

                if (result == DialogResult.No)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual Task LoadRelatedDataAsync()
        {
            // do nothing. The derived classes can customize the behaviour
            return Task.CompletedTask;
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
            this.AddReason = this.Model?.Policies?.Where(p => p.Name == nameof(CrudPolicies.Create))
                .Select(p => p.Reason).FirstOrDefault();
            this.DeleteReason = this.Model?.Policies?.Where(p => p.Name == nameof(CrudPolicies.Delete))
                .Select(p => p.Reason).FirstOrDefault();
            this.SaveReason = this.Model?.Policies?.Where(p => p.Name == nameof(CrudPolicies.Update))
                .Select(p => p.Reason).FirstOrDefault();
        }

        public async Task<bool> ExecuteSaveWithPromptAsync()
        {
            if (!this.CheckValidModel())
            {
                return false;
            }

            // TODO: will be rewritten in scope of Task
            // https://ferrettogroup.visualstudio.com/Warehouse%20Management%20System/_workitems/edit/2158
            dynamic dynamicModel = this.Model;

            if (!PolicyExtensions.CanUpdate(dynamicModel))
            {
                this.ShowErrorDialog(PolicyExtensions.GetCanUpdateReason(dynamicModel));

                return false;
            }

            this.IsBusy = true;

            var result = await this.ExecuteSaveCommandAsync();

            this.IsBusy = false;

            return result;
        }

        protected virtual bool CanExecuteDeleteCommand()
        {
            return !this.IsBusy && !this.HasConflictingChanges;
        }

        protected virtual bool CanExecuteRevertCommand()
        {
            return this.changeDetector.IsModified
                && !this.IsBusy;
        }

        protected virtual bool CanExecuteSaveCommand()
        {
            return !this.IsBusy && !this.HasConflictingChanges;
        }

        protected virtual bool CheckValidModel()
        {
            this.Model.IsValidationEnabled = true;

            return this.IsModelValid
                && this.changeDetector.IsRequiredValid;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.DeleteCommand)?.RaiseCanExecuteChanged();
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

        protected abstract Task<bool> ExecuteSaveCommandAsync();

        protected abstract Task LoadDataAsync();

        protected virtual void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateReasons();
            this.EvaluateCanExecuteCommands();
        }

        protected override void OnDispose()
        {
            if (this.modelChangedEventSubscription != null)
            {
                this.EventService.Unsubscribe<ModelChangedPubSubEvent>(this.modelChangedEventSubscription);
            }

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

        private void ChangeDetector_ModifiedChanged(object sender, EventArgs e)
        {
            if (!this.changeDetector.IsModified)
            {
                this.HasConflictingChanges = false;
            }

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

        private async Task ExecuteRefreshWithPromptAsync()
        {
            if (this.changeDetector.IsModified)
            {
                var result = this.DialogService.ShowMessage(
                    DesktopApp.AreYouSureToRefreshWithModifications,
                    DesktopApp.ConfirmOperation,
                    DialogType.Question,
                    DialogButtons.YesNo);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            await this.ExecuteRefreshCommandAsync();
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

        private async Task OnModelChangedPubSubEventAsync(ModelChangedPubSubEvent e)
        {
            var attributes = this.GetType()
                .GetCustomAttributes(typeof(ResourceAttribute), true)
                .Cast<ResourceAttribute>();

            if (this.changeDetector.IsModified)
            {
                if (e.ResourceName == e.SourceResourceName &&
                    e.ResourceId == e.SourceResourceId &&
                    attributes.Any(a => a.ResourceName == e.ResourceName && a.Primary) &&
                    this.model != null &&
                    e.ResourceId == this.model.Id.ToString())
                {
                    this.HasConflictingChanges = true;
                }
                else
                {
                    if (attributes.Any(a => a.ResourceName == e.ResourceName && !a.Primary))
                    {
                        await this.LoadRelatedDataAsync().ConfigureAwait(true);
                    }
                }
            }
            else
            {
                await this.LoadDataAsync().ConfigureAwait(true);
            }
        }

        private void SubscribeToEvents()
        {
            var attributes = this.GetType()
                .GetCustomAttributes(typeof(ResourceAttribute), true)
                .Cast<ResourceAttribute>();

            if (attributes.Any())
            {
                this.modelChangedEventSubscription = this.EventService
                    .Subscribe<ModelChangedPubSubEvent>(
                        async eventArgs => await this.OnModelChangedPubSubEventAsync(eventArgs),
                        true,
                        e => attributes.Any(a =>
                            a.ResourceName == e.ResourceName &&
                            (!a.Primary ||
                                (this.model != null && e.ResourceId == this.model.Id.ToString()))));
            }
        }

        #endregion
    }
}
