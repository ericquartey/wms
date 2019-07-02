using System;
using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ChooseLoadingUnitStepViewModel : EntityPagedListViewModel<LoadingUnit, int>, IStepNavigableViewModel
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private LoadingUnitDetails loadingUnitDetails;

        private bool hasLoadingUnits;

        private bool isLoadingCompartments;

        private bool isLoadingUnitDetailsVisible;

        private object selectedItem;

        private string title;

        #endregion

        #region Constructors

        public ChooseLoadingUnitStepViewModel(IDataSourceService dataSourceService)
            : base(dataSourceService)
        {
            this.HasLoadingUnits = true;
        }

        #endregion

        #region Properties

        public LoadingUnitDetails LoadingUnitDetails
        {
            get => this.loadingUnitDetails;
            set => this.SetProperty(ref this.loadingUnitDetails, value);
        }

        public bool HasLoadingUnits
        {
            get => this.hasLoadingUnits;
            set => this.SetProperty(ref this.hasLoadingUnits, value);
        }

        public bool IsLoadingCompartments
        {
            get => this.isLoadingCompartments;
            set => this.SetProperty(ref this.isLoadingCompartments, value);
        }

        public bool IsLoadingUnitDetailsVisible
        {
            get => this.isLoadingUnitDetailsVisible;
            set => this.SetProperty(ref this.isLoadingUnitDetailsVisible, value);
        }

        public override object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.SetProperty(ref this.selectedItem, value))
                {
                    this.RaisePropertyChanged(nameof(this.CurrentItem));
                    this.UpdateReasons();
                    this.EvaluateCanExecuteCommands();
                    this.UpdateLoadingUnitCompartmentsAsync().GetAwaiter();
                    this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));
                }
            }
        }

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }
        #endregion

        #region Methods

        public bool CanGoToNextView()
        {
            return (this.SelectedItem != null);
        }

        public bool CanSave()
        {
            return false;
        }

        public string GetError()
        {
            return null;
        }

        public virtual(string moduleName, string viewName, object data) GetNextView()
        {
            if (this.selectedItem != null)
            {
                var data = new Tuple<LoadingUnitDetails, ItemDetails>(this.loadingUnitDetails, this.Data as ItemDetails);
                return (nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.COMPARTMENTEDITSTEP, data);
            }

            return (null, null, null);
        }

        public virtual async Task<bool> SaveAsync() => await new Task<bool>(() => false);

        public async Task UpdateLoadingUnitCompartmentsAsync()
        {
            if (this.SelectedItem == null ||
                (this.SelectedItem is int notSelectedItem &&
                notSelectedItem == -1))
            {
                this.IsLoadingUnitDetailsVisible = false;
                this.LoadingUnitDetails = null;
            }
            else
            {
                this.IsLoadingCompartments = true;
                this.LoadingUnitDetails = await this.loadingUnitProvider.GetByIdAsync(((LoadingUnit)this.SelectedItem).Id);
                this.IsLoadingCompartments = false;
                this.IsLoadingUnitDetailsVisible = true;
            }
        }

        protected override async Task OnAppearAsync()
        {
            if (this.Data is ItemDetails itemDetails)
            {
                this.Title = string.Format(App.Resources.Title.AssociateCompartmentTypeToThisItem, itemDetails.Code);
            }

            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));

            await base.OnAppearAsync();

            this.SelectedFilter = this.Filters.First();

            await this.LoadDataAsync(null);
        }

        #endregion
    }
}
