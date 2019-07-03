using System.Threading.Tasks;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public class WmsWizardStepViewModel : BaseServiceNavigationViewModel, IWmsWizardStepViewModel
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #region Methods

        public virtual bool CanGoToNextView()
        {
            return false;
        }

        public virtual bool CanSave()
        {
            return false;
        }

        public virtual(string moduleName, string viewName, object data) GetNextView()
        {
            return (null, null, null);
        }

        public virtual string GetError()
        {
            return null;
        }

        public virtual async Task<bool> SaveAsync() => await new Task<bool>(() => false);

        protected override Task OnAppearAsync()
        {
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));
            return base.OnAppearAsync();
        }

        #endregion
    }
}
