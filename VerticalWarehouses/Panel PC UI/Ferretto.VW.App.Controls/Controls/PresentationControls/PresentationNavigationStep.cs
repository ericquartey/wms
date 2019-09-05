using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Controls
{
    public class PresentationNavigationStep : BasePresentation
    {
        #region Fields

        public string moduleName;

        public string viewName;

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public PresentationNavigationStep(INavigationService navigationService)
     : base(PresentationTypes.Prev)
        {
            if (navigationService is null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.navigationService = navigationService;

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
            this.navigationService = navigationService;
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
            this.navigationService.Appear(this.moduleName, this.viewName, null, false);

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
