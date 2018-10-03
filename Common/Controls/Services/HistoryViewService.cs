using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls.Services
{
    public class HistoryViewService : IHistoryViewService
    {
        #region Fields

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        #endregion Fields

        #region Methods

        public void Appear(string moduleName, string viewModelName)
        {
            var historyView = this.GetCurrentHistoryView();

            if (historyView == null)
            {
                return;
            }

            if (MvvmNaming.IsViewModelNameValid(viewModelName) == false)
            {
                return;
            }

            this.navigationService.LoadModule(moduleName);

            historyView.Appear(moduleName, viewModelName);
        }

        public void Previous()
        {
            var historyView = this.GetCurrentHistoryView();
            if (historyView != null)
            {
                historyView.Previous();
            }
        }

        private IWmsHistoryView GetCurrentHistoryView()
        {
            var elementWithFocus = Keyboard.FocusedElement as UIElement;
            return LayoutTreeHelper.GetVisualParents(elementWithFocus as DependencyObject)
                                .OfType<IWmsHistoryView>()
                                .FirstOrDefault();
        }

        #endregion Methods
    }
}
