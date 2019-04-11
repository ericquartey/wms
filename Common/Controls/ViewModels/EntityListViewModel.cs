using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public class EntityListViewModel<TModel, TKey> : BaseServiceNavigationViewModel, IEntityListViewModel
        where TModel : IModel<TKey>, IPolicyDescriptor<IPolicy>
    {
        #region Fields

        private readonly IDialogService dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

        private ICommand addCommand;

        private string addReason;

        private string deleteReason;

        private ICommand deleteCommand;

        private IEnumerable<IFilterDataSource<TModel, TKey>> filterDataSources;

        private IEnumerable<Tile> filterTiles;

        private bool flattenDataSource;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private ICommand refreshCommand;

        private string saveReason;

        private object selectedFilterDataSource;

        private Tile selectedFilterTile;

        private object selectedItem;

        #endregion

        #region Constructors

        protected EntityListViewModel()
        {
            this.InitializeEvent();
        }

        #endregion

        #region Properties

        public ICommand AddCommand => this.addCommand ??
                      (this.addCommand = new DelegateCommand(this.ExecuteAddCommand));

        public string AddReason
        {
            get => this.addReason;
            set => this.SetProperty(ref this.addReason, value);
        }

        public ColorRequired ColorRequired => ColorRequired.Default;

        public TModel CurrentItem
        {
            get
            {
                if (this.selectedItem == null)
                {
                    return default(TModel);
                }

                if (this.selectedItem is TModel)
                {
                    return (TModel)this.selectedItem;
                }

                if ((this.selectedItem is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(TModel);
                }

                return (TModel)((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedItem).OriginalRow;
            }
        }

        public string DeleteReason
        {
            get => this.deleteReason;
            set => this.SetProperty(ref this.deleteReason, value);
        }

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteWithPromptAsync()));

        public IEnumerable<Tile> Filters
        {
            get => this.filterTiles;
            protected set => this.SetProperty(ref this.filterTiles, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to skip the usage of the DevExpress InstantFeedbackSource.
        /// </summary>
        public bool FlattenDataSource
        {
            get => this.flattenDataSource;
            protected set => this.SetProperty(ref this.flattenDataSource, value);
        }

        public ICommand RefreshCommand => this.refreshCommand ??
               (this.refreshCommand = new DelegateCommand(
               this.ExecuteRefreshCommand));

        public string SaveReason
        {
            get => this.saveReason;
            set => this.SetProperty(ref this.saveReason, value);
        }

        public virtual Tile SelectedFilter
        {
            get => this.selectedFilterTile;
            set
            {
                if (this.SetProperty(ref this.selectedFilterTile, value))
                {
                    var filterDataSource = this.filterDataSources.Single(d => d.Key == value.Key);
                    this.SelectedFilterDataSource = this.flattenDataSource ? filterDataSource.GetData() : (object)filterDataSource;
                }
            }
        }

        public virtual object SelectedFilterDataSource
        {
            get => this.selectedFilterDataSource;
            protected set => this.SetProperty(ref this.selectedFilterDataSource, value);
        }

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.SetProperty(ref this.selectedItem, value))
                {
                    this.RaisePropertyChanged(nameof(this.CurrentItem));
                    this.UpdateReasons();
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        protected IDialogService DialogService => this.dialogService;

        protected IEnumerable<IFilterDataSource<TModel, TKey>> FilterDataSources => this.filterDataSources;

        #endregion

        #region Methods

        public virtual void LoadRelatedData()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    var oldFilterDataSource = this.selectedFilterDataSource;
                    this.SelectedFilterDataSource = null;
                    this.SelectedFilterDataSource = oldFilterDataSource;
                }));
        }

        public virtual async Task UpdateFilterTilesCountsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var filterTile in this.filterTiles)
                {
                    filterTile.Count = this.filterDataSources.Single(d => d.Key == filterTile.Key).GetDataCount?.Invoke();
                }
            }).ConfigureAwait(true);
        }

        public virtual void UpdateReasons()
        {
            if (this.CurrentItem is IPolicyDescriptor<IPolicy> selectedItem)
            {
                this.AddReason = selectedItem?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Create)).Select(p => p.Reason).FirstOrDefault();
                this.DeleteReason = selectedItem?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Delete)).Select(p => p.Reason).FirstOrDefault();
                this.SaveReason = selectedItem?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Update)).Select(p => p.Reason).FirstOrDefault();
            }
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
        }

        protected virtual void ExecuteAddCommand()
        {
        }

        protected virtual Task ExecuteDeleteCommandAsync()
        {
            // do nothing: derived classes can customize the behaviour of this command
            return Task.CompletedTask;
        }

        protected async Task ExecuteDeleteWithPromptAsync()
        {
            if (!this.CurrentItem.CanDelete())
            {
                this.ShowErrorDialog(this.CurrentItem.GetCanDeleteReason());
                return;
            }

            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, string.Empty),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                await this.ExecuteDeleteCommandAsync();
            }
        }

        protected void ExecuteRefreshCommand()
        {
            this.LoadRelatedData();
        }

        protected override async Task OnAppearAsync()
        {
            // TODO: check cycle because OnAppear is Async
            try
            {
                var dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
                this.filterDataSources = dataSourceService.GetAllFilters<TModel, TKey>(this.GetType().Name, this.Data);
                this.filterTiles = new BindingList<Tile>(this.filterDataSources.Select(filterDataSource => new Tile
                {
                    Key = filterDataSource.Key,
                    Name = filterDataSource.Name
                }).ToList());

                await this.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                this.EventService.Invoke(new StatusPubSubEvent(ex));
            }
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<TModel>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<TModel, TKey>>(this.modelChangedEventSubscription);

            base.OnDispose();
        }

        protected void ShowErrorDialog(string message)
        {
            this.DialogService.ShowMessage(
                message,
                DesktopApp.ConfirmOperation,
                DialogType.Warning,
                DialogButtons.OK);
        }

        private void InitializeEvent()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<TModel>>(eventArgs => { this.LoadRelatedData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<TModel, TKey>>(eventArgs => { this.LoadRelatedData(); });
        }

        #endregion
    }
}
