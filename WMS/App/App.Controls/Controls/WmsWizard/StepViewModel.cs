using System.Threading.Tasks;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public class StepViewModel : BaseServiceNavigationViewModel, IStepNavigableViewModel
    {
        #region Fields

        private string title;

        #endregion

        #region Properties

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion

        #region Methods

        public virtual bool CanGoToNextView()
        {
            return false;
        }

        public virtual bool CanSave()
        {
            return false;
        }

        public virtual string GetError()
        {
            return null;
        }

        public virtual(string moduleName, string viewName, object data) GetNextView()
        {
            return (null, null, null);
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
