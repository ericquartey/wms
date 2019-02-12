using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemAddDialogViewModel : CreateViewModel<ItemDetails>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        #endregion

        #region Methods

        protected override void ExecuteClearCommand()
        {
            this.LoadData();
        }

        // TODO: task 1256 -> protected override async Task ExecuteSaveCommand()
        protected override async Task ExecuteSaveCommand()
        {
            this.IsValidationEnabled = true;

            this.IsBusy = true;

            var result = await this.itemProvider.AddAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Item>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override void OnAppear()
        {
            base.OnAppear();

            this.LoadData();
        }

        private void LoadData()
        {
            try
            {
                this.IsBusy = true;
                this.Model = this.itemProvider.GetNew();
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
            }

            this.IsBusy = false;
        }

        #endregion
    }
}
