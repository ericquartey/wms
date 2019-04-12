using System.Windows;
using System.Windows.Input;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public class WmsMessagePopupViewModel : BaseNavigationViewModel
    {
        #region Fields

        private bool isError;

        private string message;

        private ICommand shutDownCommand;

        private string title;

        #endregion

        #region Properties

        public bool IsError
        {
            get => this.isError;
            set => this.SetProperty(ref this.isError, value);
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public ICommand ShutDownCommand => this.shutDownCommand ??
                                     (this.shutDownCommand = new DelegateCommand(
        () => { Application.Current.Shutdown(); }, this.CanShutDownCommand));

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion

        #region Methods

        internal void Update(string title, string message, bool isError)
        {
            this.Title = title;
            this.Message = message;
            this.IsError = isError;
            ((DelegateCommand)this.ShutDownCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanShutDownCommand()
        {
            return this.IsError;
        }

        #endregion
    }
}
