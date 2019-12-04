using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Regions;

namespace Ferretto.VW.App.Controls.Controls.Keyboards
{
    public class PpcKeypadsPopupViewModel : BaseNavigationViewModel
    {
        #region Fields

        private ICommand backspaceCommand;

        private ICommand closeCommand;

        private ICommand commandKey;

        private ICommand dotCommand;

        private ICommand enterCommand;

        private ICommand escCommand;

        private bool isClosed;

        private KeyboardDefinition keyboards;

        private ICommand minPlusCommand;

        private bool morePressKey;

        private string previousScreenText;

        private string screenText;

        private string title;

        #endregion

        #region Constructors

        public PpcKeypadsPopupViewModel()
        {
            var stream2 = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ferretto.VW.App.Controls.Resources.it.json"))
            {
                TextReader tr = new StreamReader(stream);
                string fileContents = tr.ReadToEnd();
                this.Keyboards = JsonConvert.DeserializeObject<KeyboardDefinition>(fileContents);
            }
        }

        #endregion

        #region Properties

        public ICommand BackspaceCommand =>
            this.backspaceCommand
            ??
            (this.backspaceCommand = new DelegateCommand(() => this.BackspaceCommandExecute()));

        public ICommand CloseCommand =>
            this.closeCommand ??
            (this.closeCommand = new DelegateCommand(() => { this.IsClosed = true; }));

        public ICommand CommandKey =>
            this.commandKey
            ??
            (this.commandKey = new DelegateCommand<string>((key) => this.KeysCommandExecute(key)));

        public ICommand DotCommand =>
            this.dotCommand
            ??
            (this.dotCommand = new DelegateCommand(() => this.KeysCommandExecute(".")));

        public ICommand EnterCommand =>
            this.enterCommand
            ??
            (this.enterCommand = new DelegateCommand(() =>
            {
                this.IsClosed = true;
            }));

        public ICommand EscCommand =>
            this.escCommand
            ??
            (this.escCommand = new DelegateCommand(() =>
            {
                this.ScreenText = this.previousScreenText;
                this.IsClosed = true;
            }));

        public bool IsClosed
        {
            get => this.isClosed;
            set => this.SetProperty(ref this.isClosed, value);
        }

        public KeyboardDefinition Keyboards
        {
            get => this.keyboards;
            set => this.SetProperty(ref this.keyboards, value);
        }

        public ICommand MinPlusCommand =>
            this.minPlusCommand
            ??
            (this.minPlusCommand = new DelegateCommand(() => this.MinPlusCommandExecute()));

        public string ScreenText
        {
            get => this.screenText;
            set => this.SetProperty(ref this.screenText, value, this.OnScreenTextPropertyChanged);
        }

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.morePressKey = false;
        }

        public void Update(string title, string value)
        {
            this.Title = title;
            this.previousScreenText = value;
            this.ScreenText = value;
        }

        private void BackspaceCommandExecute()
        {
            if (!string.IsNullOrEmpty(this.ScreenText))
            {
                if (this.morePressKey)
                {
                    this.ScreenText = this.ScreenText.Substring(0, this.ScreenText.Length - 1);
                }
                else
                {
                    this.ScreenText = string.Empty;
                }
            }
        }

        private void KeysCommandExecute(string key)
        {
            if (this.morePressKey)
            {
                this.ScreenText += key;
            }
            else
            {
                this.ScreenText = key;
            }
        }

        private void MinPlusCommandExecute()
        {
            string v = this.ScreenText;
            if (v.Substring(0, 1) == "-")
            {
                this.ScreenText = v.Replace("-", "");
            }
            else
            {
                this.ScreenText = "-" + v;
            }
        }

        private void OnScreenTextPropertyChanged()
        {
            this.morePressKey = true;
        }

        #endregion
    }
}
