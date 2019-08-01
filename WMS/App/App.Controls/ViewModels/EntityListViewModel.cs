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
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Resources;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public abstract class EntityListViewModel<TModel, TKey> : BaseServiceNavigationNotificationViewModel, IEntityListViewModel
        where TModel : IModel<TKey>, IPolicyDescriptor<IPolicy>
    {
        #region Fields

        private readonly IDataSourceService dataSourceService;

        private ICommand addCommand;

        private string addReason;

        private ICommand deleteCommand;

        private string deleteReason;

        private IEnumerable<Tile> filterTiles;

        private bool isBusy;

        private ICommand refreshCommand;

        private string saveReason;

        private object selectedFilterDataSource;

        private Tile selectedFilterTile;

        private object selectedItem;

        private ICommand showDetailsCommand;

        #endregion

        #region Constructors

        protected EntityListViewModel(IDataSourceService dataSourceService)
        {
            if (dataSourceService == null)
            {
                throw new ArgumentNullException(nameof(dataSourceService));
            }

            this.dataSourceService = dataSourceService;
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

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
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
                    this.SelectedFilterDataSource = value != null
                        ? this.FilterDataSources.Single(d => d.Key == value.Key)
                        : null;
                }
            }
        }

        public virtual object SelectedFilterDataSource
        {
            get => this.selectedFilterDataSource;
            protected set
            {
                if (this.SetProperty(ref this.selectedFilterDataSource, value)
                    &&
                    this.selectedFilterDataSource is IRefreshableDataSource refreshableSource)
                {
                    refreshableSource.RefreshAsync();
                }
            }
        }

        public virtual object SelectedItem
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

        protected IEnumerable<IDataSource<TModel, TKey>> FilterDataSources { get; private set; }

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
            // do nothing: derived classes can customize the behaviour of this method
        }

        public virtual async Task UpdateFilterTilesCountsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var filterTile in this.filterTiles)
                {
                    var dataSource = this.FilterDataSources.Single(d => d.Key == filterTile.Key);
                    if (dataSource is IFilterDataSource<TModel, TKey> filterDataSource)
                    {
                        filterTile.Count = filterDataSource.GetDataCount?.Invoke();
                    }
                    else if (dataSource is IEnumerable<TModel> enumerableDataSource)
                    {
                        filterTile.Count = enumerableDataSource.Count();
                    }
                }
            }).ConfigureAwait(true);
        }

        public virtual void UpdateReasons()
        {
            if (this.CurrentItem is IPolicyDescriptor<IPolicy> selectedItem)
            {
                this.AddReason = selectedItem?.GetCanCreateReason();
                this.DeleteReason = selectedItem?.GetCanDeleteReason();
                this.SaveReason = selectedItem?.GetCanUpdateReason();
            }
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            // do nothing: derived classes can customize the behaviour of this command
        }

        protected virtual void ExecuteAddCommand()
        {
            // do nothing: derived classes can customize the behaviour of this command
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
                var viewModelName = this.GetType().Name;
                this.FilterDataSources = this.dataSourceService.GetAllFilters<TModel, TKey>(viewModelName, this.Data);

                this.Filters = new BindingList<Tile>(this.FilterDataSources.Select(filterDataSource => new Tile
                {
                    Key = filterDataSource.Key,
                    Name = filterDataSource.Name,
                }).ToList());

                this.IsBusy = true;

                foreach (var dataSource in this.FilterDataSources)
                {
                    if (dataSource is IRefreshableDataSource refreshableDataSource)
                    {
                        await refreshableDataSource.RefreshAsync();
                    }
                }

                await this.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
                this.SelectedFilter = this.Filters.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.EventService.Invoke(new StatusPubSubEvent(ex));
            }
            finally
            {
                this.IsBusy = false;
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
