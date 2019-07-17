using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class GeneralInfoViewModel : BaseViewModel, IGeneralInfoViewModel
    {

        #region Constructors

        public GeneralInfoViewModel(
            IOtherNavigationViewModel otherNavigationViewModel)
        {
            this.OtherNavigationViewModel = otherNavigationViewModel;
            this.NavigationViewModel = otherNavigationViewModel as OtherNavigationViewModel;
        }

        #endregion

        #region Properties


        public IOtherNavigationViewModel OtherNavigationViewModel { get; }

        #endregion
    }
}
