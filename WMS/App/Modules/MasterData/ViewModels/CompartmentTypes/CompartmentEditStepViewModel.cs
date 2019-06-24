using System;
using System.Threading.Tasks;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Modules.MasterData
{
    public class CompartmentEditStepViewModel : StepViewModel
    {
        #region Methods

        protected override Task OnAppearAsync()
        {
            if (this.Data is Tuple<ItemDetails, LoadingUnit> itemLoadingUnit)
            {
                this.Title = string.Format(Ferretto.Common.Resources.Title.CreateCompartmentForThisItem, itemLoadingUnit.Item1.Code);
            }

            return base.OnAppearAsync();
        }

        #endregion
    }
}
