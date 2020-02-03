using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public class PresentationSinglePageStep : BasePresentationViewModel
    {
        #region Fields

        public string moduleName;

        public string viewName;

        #endregion

        #region Constructors

        public PresentationSinglePageStep()
            : base(PresentationTypes.PrevStep)
        {
            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
        }

        #endregion

        #region Properties

        public string ModuleName
        {
            get => this.moduleName;
            set => this.SetProperty(ref this.moduleName, value);
        }

        public string ViewName
        {
            get => this.viewName;
            set => this.SetProperty(ref this.viewName, value);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.EventAggregator
                .GetEvent<StepChangedPubSubEvent>()
                .Publish(new StepChangedMessage(this.Type == PresentationTypes.NextStep));

            return Task.CompletedTask;
        }

        protected override bool CanExecute()
        {
            if (!this.IsEnabled.HasValue)
            {
                return false;
            }

            return this.IsEnabled.Value;
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.States != null
                &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is PresentationStep prev)
            {
                this.ModuleName = prev.ModuleName;
                this.ViewName = prev.ViewName;
                if (prev.IsVisible.HasValue)
                {
                    this.IsVisible = prev.IsVisible;
                }
                if (prev.IsEnabled.HasValue)
                {
                    this.IsEnabled = prev.IsEnabled;
                }
                this.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}
