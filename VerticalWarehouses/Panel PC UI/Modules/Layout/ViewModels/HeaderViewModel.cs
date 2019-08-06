using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    public class HeaderViewModel : BasePresentationViewModel
    {
        #region Methods

        public override void InitializeData()
        {
            base.InitializeData();
            this.States.Add(this.GetInstance(nameof(PresentationTheme)));
            this.States.Add(this.GetInstance(nameof(PresentationSwitch)));
        }

        public override void UpdatePresentation(PresentationMode mode)
        {
            if (this.CurrentPresentation == PresentationMode.None ||
                this.CurrentPresentation == mode)
            {
                return;
            }

            switch (mode)
            {
                case PresentationMode.Login:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Theme, true);
                    this.Show(PresentationTypes.Switch, true);
                    break;

                case PresentationMode.Installator:
                    this.Show(PresentationTypes.None, false);
                    break;

                case PresentationMode.Operator:
                    break;

                case PresentationMode.Help:
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
