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

        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private ICommand closeCommand;

        private string inputText;

        private bool isClosed = false;

        private bool isPassword = false;

        private bool isValid = true;

        private string keyboardLayoutCode = "lowercase";

        private string labelText;

        private TimeSpan timeout = default;

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

        public TimeSpan InactiveTimeout
        {
            get => this.timeout;
            set
            {
                if (this.SetProperty(ref this.timeout, value))
                {
                    this.ResetTimer();
                }
            }
        }

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

        public bool IsPassword
        {
            get => this.isPassword;
            set => this.SetProperty(ref this.isPassword, value);
        }

        public bool IsValid
        {
            get => this.isValid;
            set => this.SetProperty(ref this.isValid, value);
        }

        public string KeyboardLayoutCode
        {
            get => this.keyboardLayoutCode;
            set => this.SetProperty(ref this.keyboardLayoutCode, value);
        }

        public string LabelText
        {
            get => this.labelText;
            set => this.SetProperty(ref this.labelText, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.StopTimer();
            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
            this.ResetTimer();

            return base.OnAppearedAsync();
        }

        public void ResetTimer()
        {
            this.StopTimer();
            if (this.dispatcherTimer.IsEnabled = (this.dispatcherTimer.Interval = this.InactiveTimeout) != TimeSpan.Zero)
            {
                this.dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.IsClosed = true;
        }

        private void StopTimer()
        {
            this.dispatcherTimer.Stop();
        }

        #endregion
    }
}
