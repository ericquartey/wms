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
    public class ItemListAddDialogViewModel : CreateViewModel<ItemListDetails>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        #endregion

        #region Methods

        protected override void ExecuteClearCommand()
        {
            this.LoadData();
        }

        // TODO: task 1256 -> protected override async Task ExecuteSaveCommand()
        protected override Task ExecuteCreateCommand()
        {
            this.IsValidationEnabled = true;
            throw new NotImplementedException();
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            this.LoadData();
        }

        private void LoadData()
        {
            try
            {
                this.IsBusy = true;
                this.Model = this.itemListProvider.GetNew();
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
