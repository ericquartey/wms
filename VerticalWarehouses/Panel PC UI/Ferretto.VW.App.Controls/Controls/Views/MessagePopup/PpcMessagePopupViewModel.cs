using System.Windows.Input;
using Prism.Commands;

namespace Ferretto.VW.App.Controls
{
    public class PpcMessagePopupViewModel : BaseNavigationViewModel
    {
        #region Fields

        private ICommand closeCommand;

        private bool isClosed;

        private string message;

        private string title;

        #endregion

        #region Properties

        public ICommand CloseCommand => this.closeCommand ??
                                     (this.closeCommand = new DelegateCommand(
        () => { this.IsClosed = true; ; }));

        public bool IsClosed
        {
            get => this.isClosed;
            set => this.SetProperty(ref this.isClosed, value);
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion

        #region Methods

        public void Update(string title, string message)
        {
            this.Title = title;
            this.Message = message;
        }

        #endregion
    }
}
