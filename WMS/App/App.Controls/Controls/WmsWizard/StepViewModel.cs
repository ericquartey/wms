using System.Threading.Tasks;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public class StepViewModel : BaseServiceNavigationViewModel, IStepNavigableViewModel
    {
        #region Methods

        public virtual bool CanGoToNextView()
        {
            return false;
        }

        public virtual(string moduleName, string viewName, object data) GetNextView()
        {
            return (null, null, null);
        }

        public virtual bool Save()
        {
            return true;
        }

        protected override Task OnAppearAsync()
        {
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));
            return base.OnAppearAsync();
        }

        #endregion
    }
}
