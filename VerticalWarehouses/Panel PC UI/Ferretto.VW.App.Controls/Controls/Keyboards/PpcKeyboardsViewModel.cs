using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;

namespace Ferretto.VW.App.Controls.Keyboards
{
    public class PpcKeyboardsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private ICommand closeCommand;

        private string inputText;

        private bool isClosed = false;

        private string keyboardLayoutCode = "lowercase";

        #endregion

        #region Constructors

        public PpcKeyboardsViewModel(string layoutCode) : base()
        {
            this.keyboardLayoutCode = layoutCode ?? throw new ArgumentNullException(nameof(layoutCode));
        }

        #endregion

        #region Properties

        public ICommand CloseCommand =>
            this.closeCommand ??
            (this.closeCommand = new DelegateCommand(() => { this.IsClosed = true; }));

        public string InputText
        {
            get => this.inputText;
            set => this.SetProperty(ref this.inputText, value);
        }

        public bool IsClosed
        {
            get => this.isClosed;
            set => this.SetProperty(ref this.isClosed, value);
        }

        public string KeyboardLayoutCode
        {
            get => this.keyboardLayoutCode;
            set => this.SetProperty(ref this.keyboardLayoutCode, value);
        }

        #endregion
    }
}
