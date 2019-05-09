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
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public abstract class EntityListViewModel<TModel, TKey> : BaseServiceNavigationNotificationViewModel<TModel, TKey>, IEntityListViewModel
        where TModel : IModel<TKey>, IPolicyDescriptor<IPolicy>
    {
        #region Fields

        private ICommand addCommand;

        private string addReason;

        private ICommand deleteCommand;

        private string deleteReason;

        private IEnumerable<Tile> filterTiles;

        private bool flattenDataSource;

        private ICommand refreshCommand;

        private string saveReason;

        private object selectedFilterDataSource;

        private Tile selectedFilterTile;

        private object selectedItem;

        private ICommand showDetailsCommand;

        #endregion

        #region Constructors

        protected EntityListViewModel()
        {
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

                if ((this.selectedItem is DevExpress.Data.Async.Helpers
                    .ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(TModel);
                }

                return (TModel)((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this
                    .selectedItem).OriginalRow;
            }
        }

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteWithPromptAsync(),
                this.CanDelete)
            .ObservesProperty(() => this.SelectedItem));

        public string DeleteReason
        {
            get => this.deleteReason;
            set => this.SetProperty(ref this.deleteReason, value);
        }

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
                    var filterDataSource = this.FilterDataSources.Single(d => d.Key == value.Key);
                    this.SelectedFilterDataSource =
                        this.flattenDataSource ? filterDataSource.GetData() : (object)filterDataSource;
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

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                                            (this.showDetailsCommand = new DelegateCommand(
                    this.ShowDetails,
                    this.CanShowDetails)
                .ObservesProperty(() => this.SelectedItem));

        protected IDialogService DialogService { get; } = ServiceLocator.Current.GetInstance<IDialogService>();

        protected IEnumerable<IFilterDataSource<TModel, TKey>> FilterDataSources { get; private set; }

        #endregion

        #region Methods

        public virtual bool CanDelete()
        {
            return this.SelectedItem != null;
        }

        public virtual bool CanShowDetails()
        {
            return this.SelectedItem != null;
        }

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

        public virtual void ShowDetails()
        {
        }

        public virtual async Task UpdateFilterTilesCountsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var filterTile in this.filterTiles)
                {
                    filterTile.Count = this.FilterDataSources.Single(d => d.Key == filterTile.Key).GetDataCount
                        ?.Invoke();
                }
            }).ConfigureAwait(true);
        }

        public virtual void UpdateReasons()
        {
            if (this.CurrentItem is IPolicyDescriptor<IPolicy> selectedItem)
            {
                this.AddReason = selectedItem?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Create))
                    .Select(p => p.Reason).FirstOrDefault();
                this.DeleteReason = selectedItem?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Delete))
                    .Select(p => p.Reason).FirstOrDefault();
                this.SaveReason = selectedItem?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Update))
                    .Select(p => p.Reason).FirstOrDefault();
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
                this.FilterDataSources = dataSourceService.GetAllFilters<TModel, TKey>(this.GetType().Name, this.Data);
                this.filterTiles = new BindingList<Tile>(this.FilterDataSources.Select(filterDataSource => new Tile
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

        protected void ShowErrorDialog(string message)
        {
            this.DialogService.ShowMessage(
                message,
                DesktopApp.ConfirmOperation,
                DialogType.Warning,
                DialogButtons.OK);
        }

        #endregion
    }
}
