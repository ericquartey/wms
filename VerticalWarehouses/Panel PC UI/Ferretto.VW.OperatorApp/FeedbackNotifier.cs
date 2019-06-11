using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;

namespace Ferretto.VW.OperatorApp
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

        public async void Notify(string s)
        {
            this.container.Resolve<IMainWindowBackToOAPPButtonViewModel>().Note = s;
            await this.CallBackMethod();
        }

        private async Task CallBackMethod()
        {
            await Task.Delay(2000);
            this.container.Resolve<IMainWindowBackToOAPPButtonViewModel>().Note = string.Empty;
        }

        #endregion
    }
}
