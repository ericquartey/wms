using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class StatusBarViewModel : BindableBase
    {
        private string info;

        public StatusBarViewModel()
        {
        }

        public string Info
        {
            get => this.info;
            set => this.SetProperty(ref this.info, value);
        }
    }
}
