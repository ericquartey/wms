using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Prism.Commands;
using Prism.Events;
using Unity;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels
{
    public class FooterViewModel : BaseViewModel, IFooterViewModel
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly IStatusMessageService statusMessageService;

        private ICommand navigateBackCommand;

        private string note;

        private Brush noteBrush;

        #endregion

        #region Constructors

        public FooterViewModel(
            IEventAggregator eventAggregator,
            IStatusMessageService statusMessageService,
            IUnityContainer container)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (statusMessageService == null)
            {
                throw new System.ArgumentNullException(nameof(statusMessageService));
            }

            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            this.eventAggregator = eventAggregator;
            this.container = container;
            this.NavigationViewModel = null;

            this.statusMessageService = statusMessageService;
            this.statusMessageService.StatusMessageNotified += this.StatusMessageService_StatusMessageNotified;
        }

        #endregion

        #region Properties

        public ICommand NavigateBackCommand =>
            this.navigateBackCommand
            ??
            (this.navigateBackCommand = new DelegateCommand(
                () => this.container.Resolve<Operator.Interfaces.INavigationService>().NavigateFromView()));

        public string Note { get => this.note; set => this.SetProperty(ref this.note, value); }

        public Brush NoteBrush
        {
            get => this.noteBrush;
            set => this.SetProperty(ref this.noteBrush, value);
        }

        #endregion

        #region Methods

        public void FinalizeBottomButtons()
        {
            this.navigateBackCommand = null;
        }

        private void StatusMessageService_StatusMessageNotified(object sender, StatusMessageEventArgs e)
        {
            this.Note = e.Message;

            switch (e.Level)
            {
                case StatusMessageLevel.Error:
                    this.NoteBrush = Brushes.DarkRed;
                    break;

                case StatusMessageLevel.Success:
                    this.NoteBrush = Brushes.Green;
                    break;

                case StatusMessageLevel.Warning:
                    this.NoteBrush = Brushes.Gold;
                    break;

                default:
                    this.NoteBrush = Brushes.White;
                    break;
            }
        }

        #endregion
    }
}
