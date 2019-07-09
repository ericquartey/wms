using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;

namespace Ferretto.VW.OperatorApp
{
    public class FeedbackNotifier : IFeedbackNotifier
    {
        #region Fields

        private readonly IFooterViewModel footerViewModel;

        #endregion

        #region Constructors

        public FeedbackNotifier(IFooterViewModel footerViewModel)
        {
            this.footerViewModel = footerViewModel;
        }

        #endregion

        #region Methods

        public async void Notify(string s)
        {
            this.footerViewModel.Note = s;
            await this.CallBackMethod();
        }

        private async Task CallBackMethod()
        {
            await Task.Delay(2000);
            this.footerViewModel.Note = string.Empty;
        }

        #endregion
    }
}
