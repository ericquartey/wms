using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ChooseLoadingUnitStepViewModel : WmsWizardStepViewModel
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider =
            ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private bool hasLoadingUnits;

        private bool isLoadingCompartments;

        private bool isLoadingUnitDetailsVisible;

        private int itemId;

        private LoadingUnitDetails loadingUnitDetails;

        private InfiniteAsyncSource loadingUnitsDataSource;

        private object selectedItem;

        #endregion

        #region Constructors

        public ChooseLoadingUnitStepViewModel()
        {
            this.HasLoadingUnits = true;
        }

        #endregion

        #region Properties

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

        public LoadingUnitDetails LoadingUnitDetails
        {
            get => this.loadingUnitDetails;
            set => this.SetProperty(ref this.loadingUnitDetails, value);
        }

        public InfiniteAsyncSource LoadingUnitsDataSource
        {
            get => this.loadingUnitsDataSource;
            set => this.SetProperty(ref this.loadingUnitsDataSource, value);
        }

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.SetProperty(ref this.selectedItem, value))
                {
                    this.UpdateLoadingUnitCompartmentsAsync().GetAwaiter();
                    this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));
                }
            }
        }

        #endregion

        #region Methods

        public override bool CanGoToNextView()
        {
            return this.SelectedItem != null;
        }

        public override(string moduleName, string viewName, object data) GetNextView()
        {
            if (this.selectedItem != null)
            {
                var data = new Tuple<LoadingUnitDetails, ItemDetails>(
                    this.loadingUnitDetails,
                    this.Data as ItemDetails);
                return (nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.COMPARTMENTEDITSTEP,
                        data);
            }

            return (null, null, null);
        }

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
                this.LoadingUnitDetails =
                    await this.loadingUnitProvider.GetByIdAsync(((LoadingUnit)this.SelectedItem).Id);
                this.IsLoadingCompartments = false;
                this.IsLoadingUnitDetailsVisible = true;
            }
        }

        protected override async Task OnAppearAsync()
        {
            if (this.Data is ItemDetails itemDetails)
            {
                this.Title = string.Format(App.Resources.Title.AssociateCompartmentTypeToThisItem, itemDetails.Code);
                this.itemId = itemDetails.Id;

                Func<int, int, IEnumerable<SortOption>, Task<IEnumerable<LoadingUnit>>> getAllAllowedByItemIdAsync =
                    this.GetAllAllowedByItemIdAsync;
                this.LoadingUnitsDataSource = new InfiniteDataSourceService<LoadingUnit, int>(
                    this.loadingUnitProvider, getAllAllowedByItemIdAsync).DataSource;
            }

            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));

            await base.OnAppearAsync();
        }

        private async Task<IEnumerable<LoadingUnit>> GetAllAllowedByItemIdAsync(
            int skip,
            int pageSize,
            IEnumerable<SortOption> sortOrder)
        {
            var result =
                await this.loadingUnitProvider.GetAllAllowedByItemIdAsync(this.itemId, skip, pageSize, sortOrder);
            return !result.Success ? null : result.Entity;
        }

        #endregion
    }
}
