using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using CommonServiceLocator;

namespace Ferretto.Common.Controls.Services
{
    public class HistoryViewService : IHistoryViewService
    {
        #region Fields

        private readonly IInputService inputService;

        private readonly INavigationService navigationService;

        private IWmsHistoryView currentHistoryView;

        private bool isControlPressed;

        #endregion

        #region Constructors

        public HistoryViewService(IInputService inputService, INavigationService navigationService)
        {
            this.inputService = inputService;
            this.navigationService = navigationService;
            this.inputService.BeginMouseNotify(this, this.OnMouseDown);
        }

        #endregion

        #region Properties

        public IInputService InputService => this.inputService;

        #endregion

        #region Methods

        public void Appear(string moduleName, string viewModelName, object data = null)
        {
            if (this.currentHistoryView == null)
            {
                return;
            }

            if (MvvmNaming.IsViewModelNameValid(viewModelName) == false)
            {
                return;
            }

            try
            {
                this.navigationService.LoadModule(moduleName);

                if (this.isControlPressed)
                {
                    this.navigationService.Appear(moduleName, viewModelName, data);
                }
                else
                {
                    this.currentHistoryView.Appear(moduleName, viewModelName, data);
                }

                this.Reset();
            }
            catch (System.Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, $"Cannot show view {viewModelName} for module {moduleName}.");
            }
        }

        public void Previous()
        {
            this.currentHistoryView?.Previous();
            this.Reset();
        }

        private void OnMouseDown(MouseDownInfo mouseDownInfo)
        {
            this.currentHistoryView = LayoutTreeHelper.GetVisualParents(mouseDownInfo.OriginalSource as DependencyObject)
                                .OfType<IWmsHistoryView>()
                                .FirstOrDefault();
            this.isControlPressed = (Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control;
        }

        private void Reset()
        {
            this.currentHistoryView = null;
            this.isControlPressed = false;
        }

        #endregion
    }
}
