using Ferretto.VW.InstallationApp.Interfaces;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class FeedbackNotifier : IFeedbackNotifier
    {
        #region Fields

        private IUnityContainer container;

        #endregion

        #region Methods

        public void Initialize(IUnityContainer container)
        {
            this.container = container;
        }

        //TEMP Maybe not necessary
        //public async void Notify(string s)
        public void Notify(string s)
        {
            this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>().Note = s;

            //TEMP Maybe not necessary
            //await this.CallBackMethod();
        }

        #endregion

        //TEMP Maybe not necessary
        //private async Task CallBackMethod()
        //{
        //    await Task.Delay(2000);
        //    this.container.Resolve<IMainWindowBackToIAPPButtonViewModel>().Note = string.Empty;
        //}
    }
}
