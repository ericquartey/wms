using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListRowDetailsViewModel : DetailsViewModel<ItemListRowDetails>
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();
        private ItemListRowDetails itemListRow;

        #endregion Fields

        #region Properties

        public ItemListRowDetails ItemListRow
        {
            get => this.itemListRow;
            set
            {
                if (!this.SetProperty(ref this.itemListRow, value))
                {
                    return;
                }

                this.TakeSnapshot(this.ItemListRow);

                //TODO
                //this.RefreshData();
            }
        }

        #endregion Properties

        #region Methods

        protected override Task ExecuteRevertCommand()
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteSaveCommand()
        {
            throw new NotImplementedException();
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
        }

        private async Task LoadData()
        {
            if ((this.Data is int modelId))
            {
                this.ItemListRow = await this.itemListRowProvider.GetById(modelId);
            }
        }

        #endregion Methods
    }
}
